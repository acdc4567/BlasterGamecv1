// Fill out your copyright notice in the Description page of Project Settings.


#include "BlasterComponents/CombatComponent.h"
#include "Weapons/Weapon.h"
#include "Character/BlasterCharacter.h"
#include "Engine/SkeletalMeshSocket.h"
#include "Components/SphereComponent.h"
#include "Net/UnrealNetwork.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Kismet/GameplayStatics.h"
#include "DrawDebugHelpers.h"
#include "PlayerControllers/BlasterPlayerController.h"
#include "Camera/CameraComponent.h"
#include "TimerManager.h"
#include "Sound/SoundCue.h"
#include "Weapons/Projectile.h"
#include "Weapons/Shotgun.h"





// Sets default values for this component's properties
UCombatComponent::UCombatComponent() {
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = 1;

	// ...
}

void UCombatComponent::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const {
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);
	DOREPLIFETIME(UCombatComponent, EquippedWeapon);
	DOREPLIFETIME(UCombatComponent, SecondaryWeapon);

	DOREPLIFETIME(UCombatComponent, bAiming);
	DOREPLIFETIME_CONDITION(UCombatComponent, CarriedAmmo,COND_OwnerOnly);
	DOREPLIFETIME(UCombatComponent, CombatState);


}

// Called when the game starts
void UCombatComponent::BeginPlay() {
	Super::BeginPlay();

	if (Character) {
		Character->GetCharacterMovement()->MaxWalkSpeed = BaseWalkSpeed;

		if (Character->GetFollowCamera()) {
			DefaultFOV = Character->GetFollowCamera()->FieldOfView;
			CurrentFOV = DefaultFOV;

		}

		if (Character->HasAuthority()) {
			InitializeCarriedAmmo();
		}


	}



}

void UCombatComponent::SetAiming(bool bIsAiming) {
	if (Character == nullptr || EquippedWeapon == nullptr)return;




	bAiming = bIsAiming;

	Server_SetAiming(bIsAiming);
	if (Character) {
		Character->GetCharacterMovement()->MaxWalkSpeed = bIsAiming ? AimWalkSpeed : BaseWalkSpeed;
	}

	if (Character->IsLocallyControlled() && EquippedWeapon->GetWeaponType() == E_WeaponType::EWT_SniperRifle) {
		Character->ShowSniperScopeWidget(bIsAiming);


	}

	if(Character->IsLocallyControlled()) bAimButtonPressed = bIsAiming;

}

void UCombatComponent::OnRep_EquippedWeapon() {

	if (EquippedWeapon && Character) {
		EquippedWeapon->SetWeaponState(E_WeaponState::EWS_Equipped);

		AttachActorToRightHand(EquippedWeapon);
		
		Character->GetCharacterMovement()->bOrientRotationToMovement = 0;
		Character->bUseControllerRotationYaw = 1;
		PlayEquipWeaponSound(EquippedWeapon);
		EquippedWeapon->EnableCustomDepth(0);
		EquippedWeapon->SetHUDAmmo();
	}




}

void UCombatComponent::OnRep_SecondaryWeapon() {

	if (SecondaryWeapon && Character) {
		SecondaryWeapon->SetWeaponState(E_WeaponState::EWS_EquippedSecondary);
		AttachActorToBackpack(SecondaryWeapon);
		PlayEquipWeaponSound(SecondaryWeapon);
		
		

	}



}

void UCombatComponent::FireButtonPressed(bool bPressed) {
	
	
	bFireButtonPressed = bPressed;
	if (bFireButtonPressed) {
		
		Fire();

	}
	

}

void UCombatComponent::FinishThrowGrenade() {
	CombatState = E_CombatState::ECS_UnOccupied;
	AttachActorToRightHand(EquippedWeapon);


}

void UCombatComponent::LaunchGrenade() {
	ShowAttachedGrenade(0);

	if (Character && Character->IsLocallyControlled()) {
		Server_LaunchGrenade(HitTarget);


	}
	




}

void UCombatComponent::Server_LaunchGrenade_Implementation(const FVector_NetQuantize& Target) {
	if (Character && GrenadeClass && Character->GetAttachedGrenade()) {
		const FVector StartingLocation = Character->GetAttachedGrenade()->GetComponentLocation() + FVector(15.f, 0.f, 15.f);
		FVector ToTarget = Target - StartingLocation;
		FActorSpawnParameters SpawnParams;
		SpawnParams.Owner = Character;
		SpawnParams.Instigator = Character;
		UWorld* World = GetWorld();
		if (World) {
			World->SpawnActor<AProjectile>(GrenadeClass, StartingLocation, ToTarget.Rotation(), SpawnParams);;


		}


	}


}

void UCombatComponent::Fire() {
	if (CanFire()) {
		
		

		if (EquippedWeapon) {
			bCanFire = 0;
			CrosshairShootingFactor = .7f;

			switch (EquippedWeapon->FireType) {
			case E_FireType::EFT_HitScan:
				FireHitScanWeapon();

				break;
			case E_FireType::EFT_Projectile:
				FireProjectileWeapon();

				break;
			case E_FireType::EFT_Shotgun:

				FireShotgun();
				break;
			case E_FireType::EFT_MAX:
				break;
			default:
				break;
			}








		}

		StartFireTimer();
	}

	
}

void UCombatComponent::FireProjectileWeapon() {
	
	if (EquippedWeapon&&Character) {
		HitTarget = EquippedWeapon->bUseScatter ? EquippedWeapon->TraceEndWithScatter(HitTarget) : HitTarget;

		if(!Character->HasAuthority()) LocalFire(HitTarget);
		Server_Fire(HitTarget);

	}
	


}

void UCombatComponent::FireHitScanWeapon() {
	if (EquippedWeapon && Character) {

		HitTarget = EquippedWeapon->bUseScatter ? EquippedWeapon->TraceEndWithScatter(HitTarget) : HitTarget;
		if (!Character->HasAuthority())LocalFire(HitTarget);
		Server_Fire(HitTarget);

	}



}

void UCombatComponent::FireShotgun() {
	if (EquippedWeapon && Character) {
		AShotgun* Shotgun = Cast<AShotgun>(EquippedWeapon);
		if (Shotgun) {
			TArray<FVector_NetQuantize> HitTargets;
			Shotgun->ShotgunTraceEndWithScatter(HitTarget, HitTargets);

			if (!Character->HasAuthority())LocalShotgunFire(HitTargets);
			Server_ShotgunFire(HitTargets);


		}
		


	}
	






}

void UCombatComponent::LocalFire(const FVector_NetQuantize& TraceHitTarget) {

	if (EquippedWeapon == nullptr) {
		return;
	}

	

	if (Character && CombatState == E_CombatState::ECS_UnOccupied) {

		Character->PlayFireMonatge(bAiming);
		EquippedWeapon->Fire(TraceHitTarget);
	}




}

void UCombatComponent::LocalShotgunFire(const TArray<FVector_NetQuantize>& TraceHitTargets) {
	
	AShotgun* Shotgun = Cast<AShotgun>(EquippedWeapon);
	


	if (Shotgun == nullptr||Character==nullptr)return;
	if (CombatState == E_CombatState::ECS_UnOccupied ) {
		Character->PlayFireMonatge(bAiming);
		Shotgun->FireShotgun(TraceHitTargets);

	}

}




void UCombatComponent::Server_Fire_Implementation(const FVector_NetQuantize& TraceHitTarget) {

	Multicast_Fire(TraceHitTarget);



}



void UCombatComponent::Multicast_Fire_Implementation(const FVector_NetQuantize& TraceHitTarget) {
	if (Character && Character->IsLocallyControlled() && !Character->HasAuthority())return;
	LocalFire(TraceHitTarget);

}



void UCombatComponent::Server_SetAiming_Implementation(bool bIsAiming) {

	bAiming = bIsAiming;
	if (Character) {
		Character->GetCharacterMovement()->MaxWalkSpeed = bIsAiming ? AimWalkSpeed : BaseWalkSpeed;
	}

}





void UCombatComponent::Server_ShotgunFire_Implementation(const TArray<FVector_NetQuantize>& TraceHitTargets) {
	Multicast_ShotgunFire(TraceHitTargets);

}

void UCombatComponent::Multicast_ShotgunFire_Implementation(const TArray<FVector_NetQuantize>& TraceHitTargets) {
	if (Character && Character->IsLocallyControlled() && !Character->HasAuthority())return;
	LocalShotgunFire(TraceHitTargets);

}

void UCombatComponent::TraceUnderCrosshairs(FHitResult& TraceHitResult) {
	FVector2D ViewportSize;
	if (GEngine && GEngine->GameViewport) {

		GEngine->GameViewport->GetViewportSize(ViewportSize);

	}
	FVector2D CrosshairLocation(ViewportSize.X / 2.f, ViewportSize.Y / 2.f);

	FVector CrosshairWorldPosition;
	FVector CrosshairWorldDirection;

	bool bScreenToWorld= UGameplayStatics::DeprojectScreenToWorld(UGameplayStatics::GetPlayerController(this, 0), CrosshairLocation, CrosshairWorldPosition, CrosshairWorldDirection);
	if (bScreenToWorld) {

		FVector Start = CrosshairWorldPosition;
		if (Character) {
			float DistanceToCharacter = (Character->GetActorLocation() - Start).Size();
			Start += CrosshairWorldDirection * (DistanceToCharacter + 60.f);


		}


		FVector End = Start + CrosshairWorldDirection * TRACE_LENGTH;

		GetWorld()->LineTraceSingleByChannel(TraceHitResult, Start, End, ECollisionChannel::ECC_Visibility);
		if (!TraceHitResult.bBlockingHit) {
			TraceHitResult.ImpactPoint = End;
			//HitTarget = End;
		}
		if (TraceHitResult.GetActor() && TraceHitResult.GetActor()->Implements<UInteractWCrosshairsInterface>()) {
			HUDPackage.CrosshairsColor = FLinearColor::Red;
		}
		else {
			HUDPackage.CrosshairsColor = FLinearColor::Black;

		}




		

	}



}

void UCombatComponent::SetHUDCrosshairs(float DeltaTime) {

	if (!Character || Character->Controller == nullptr)return;

	Controller = Controller == nullptr ? Cast<ABlasterPlayerController>(Character->Controller) : Controller;
	if (Controller) {
		HUD = HUD == nullptr ? Cast<ABlasterHUD>(Controller->GetHUD()) : HUD;
		
		
		
		if (HUD) {
			
			if (EquippedWeapon) {
				if (EquippedWeapon->GetWeaponType() == E_WeaponType::EWT_SniperRifle && Character->GetIsAiming()) {
					HUDPackage.CrosshairsCenter = nullptr;
					HUDPackage.CrosshairsLeft = nullptr;
					HUDPackage.CrosshairsRight = nullptr;
					HUDPackage.CrosshairsTop = nullptr;
					HUDPackage.CrosshairsBottom = nullptr;


				}
				else {

					HUDPackage.CrosshairsCenter = EquippedWeapon->CrosshairsCenter;
					HUDPackage.CrosshairsLeft = EquippedWeapon->CrosshairsLeft;
					HUDPackage.CrosshairsRight = EquippedWeapon->CrosshairsRight;
					HUDPackage.CrosshairsTop = EquippedWeapon->CrosshairsTop;
					HUDPackage.CrosshairsBottom = EquippedWeapon->CrosshairsBottom;
				}

				
			}
			else {
				HUDPackage.CrosshairsCenter = nullptr;
				HUDPackage.CrosshairsLeft = nullptr;
				HUDPackage.CrosshairsRight = nullptr;
				HUDPackage.CrosshairsTop = nullptr;
				HUDPackage.CrosshairsBottom = nullptr;
			}
			
			//Calculate Crosshair Spread

			FVector2D WalkSpeedRange(0.f, Character->GetCharacterMovement()->MaxWalkSpeed);
			FVector2D VelocityMultiplierRange(0.f, 1.f);
			FVector Velocity = Character->GetVelocity();
			Velocity.Z = 0.f;

			CrosshairVelocityFactor = FMath::GetMappedRangeValueClamped(WalkSpeedRange, VelocityMultiplierRange, Velocity.Size());

			if (Character->GetCharacterMovement()->IsFalling()) {
				CrosshairInAirFactor = FMath::FInterpTo(CrosshairInAirFactor, 2.25f, DeltaTime, 2.25f);
			}
			else
			{
				CrosshairInAirFactor = FMath::FInterpTo(CrosshairInAirFactor, 0.f, DeltaTime, 32.25f);
			}

			if (bAiming) {
				CrosshairAimFactor = FMath::FInterpTo(CrosshairAimFactor, .58f, DeltaTime, 30.f);
			}
			else {
				CrosshairAimFactor = FMath::FInterpTo(CrosshairAimFactor, 0.f, DeltaTime, 30.f);

			}

			CrosshairShootingFactor = FMath::FInterpTo(CrosshairShootingFactor, 0.f, DeltaTime, 30.f);




			HUDPackage.CrosshairSpread = .5f + CrosshairVelocityFactor + CrosshairInAirFactor - CrosshairAimFactor + CrosshairShootingFactor;

			HUD->SetHUDPackage(HUDPackage);




		}
		
		


	}

	


}

void UCombatComponent::HandleReload() {

	if (Character) {

		Character->PlayReloadMonatge();

	}
	

	
}

int32 UCombatComponent::AmountToReload() {
	if (EquippedWeapon == nullptr)return 0;
	int32 RoomInMag = EquippedWeapon->GetMagCapacity() - EquippedWeapon->GetAmmo();

	if (CarriedAmmoMap.Contains(EquippedWeapon->GetWeaponType())) {
		
		int32 AmountCarried = CarriedAmmoMap[EquippedWeapon->GetWeaponType()];
		int32 Least = FMath::Min(RoomInMag, AmountCarried);
		return FMath::Clamp(RoomInMag, 0, Least);
	}


	return 0;



	

}

void UCombatComponent::ThrowGrenade() {
	if (CombatState != E_CombatState::ECS_UnOccupied || EquippedWeapon == nullptr) return;

	CombatState = E_CombatState::ECS_ThrowingGrenade;

	if (Character) {

		Character->PlayThrowGrenadeMonatge();
		AttachActorToLeftHand(EquippedWeapon);
		ShowAttachedGrenade(1);
	}

	if (Character && !Character->HasAuthority()) {
		Server_ThrowGrenade();


	}



}

void UCombatComponent::DropEquippedWeapon() {
	if (EquippedWeapon) {
		EquippedWeapon->Dropped();
	}


}

void UCombatComponent::AttachActorToRightHand(AActor* ActorToAttach) {
	if (Character == nullptr || Character->GetMesh() == nullptr || ActorToAttach == nullptr)return;
	const USkeletalMeshSocket* HandSocket = Character->GetMesh()->GetSocketByName("RightHandSocket");
	if (HandSocket) {
		HandSocket->AttachActor(ActorToAttach, Character->GetMesh());
	}


}

void UCombatComponent::AttachActorToLeftHand(AActor* ActorToAttach) {

	if (Character == nullptr || Character->GetMesh() == nullptr || ActorToAttach == nullptr || EquippedWeapon == nullptr)return;
	const USkeletalMeshSocket* HandSocket = Character->GetMesh()->GetSocketByName("LeftHandSocket");
	if (HandSocket) {
		HandSocket->AttachActor(ActorToAttach, Character->GetMesh());
	}

}

void UCombatComponent::AttachActorToBackpack(AActor* ActorToAttach) {
	if (Character == nullptr || Character->GetMesh() == nullptr || ActorToAttach == nullptr)return;
	const USkeletalMeshSocket* BackpackSocket = Character->GetMesh()->GetSocketByName("BackpackSocket");
	if (BackpackSocket) {
		BackpackSocket->AttachActor(ActorToAttach, Character->GetMesh());
	}



}

void UCombatComponent::UpdateCarriedAmmo() {
	if (EquippedWeapon == nullptr)return;

	if (CarriedAmmoMap.Contains(EquippedWeapon->GetWeaponType())) {
		CarriedAmmo = CarriedAmmoMap[EquippedWeapon->GetWeaponType()];
	}

	Controller = Controller == nullptr ? Cast<ABlasterPlayerController>(Character->Controller) : Controller;
	if (Controller) {

		Controller->SetHUDCarriedAmmo(CarriedAmmo);

	}



}

void UCombatComponent::PlayEquipWeaponSound(AWeapon* WeaponToEquip) {

	if (Character && EquippedWeapon && EquippedWeapon->EquipSound && WeaponToEquip) {
		UGameplayStatics::PlaySoundAtLocation(this, EquippedWeapon->EquipSound, Character->GetActorLocation());
	}



}

void UCombatComponent::ReloadEmptyWeapon() {

	if (EquippedWeapon && EquippedWeapon->IsEmpty()) {
		Reload();
	}


}

void UCombatComponent::ShowAttachedGrenade(bool bShowGrenade) {

	if (Character && Character->GetAttachedGrenade()) {
		Character->GetAttachedGrenade()->SetVisibility(bShowGrenade);


	}




}

void UCombatComponent::EquipPrimaryWeapon(AWeapon* WeaponToEquip) {
	
	if (WeaponToEquip == nullptr)return;

	DropEquippedWeapon();

	EquippedWeapon = WeaponToEquip;

	EquippedWeapon->SetWeaponState(E_WeaponState::EWS_Equipped);

	AttachActorToRightHand(EquippedWeapon);


	EquippedWeapon->SetOwner(Character);

	EquippedWeapon->SetHUDAmmo();


	UpdateCarriedAmmo();

	PlayEquipWeaponSound(WeaponToEquip);

	ReloadEmptyWeapon();

	



}

void UCombatComponent::EquipSecondaryWeapon(AWeapon* WeaponToEquip) {
	if (WeaponToEquip == nullptr)return;

	SecondaryWeapon = WeaponToEquip;
	SecondaryWeapon->SetWeaponState(E_WeaponState::EWS_EquippedSecondary);
	AttachActorToBackpack(WeaponToEquip);
	SecondaryWeapon->SetOwner(Character);
	PlayEquipWeaponSound(WeaponToEquip);

	

	

}

void UCombatComponent::OnRep_Aiming() {
	if (Character && Character->IsLocallyControlled()) {
		bAiming = bAimButtonPressed;

	}


}

void UCombatComponent::PickupAmmo(E_WeaponType WeaponType, int32 AmmoAmount) {

	if (CarriedAmmoMap.Contains(WeaponType)) {
		CarriedAmmoMap[WeaponType] = FMath::Clamp(CarriedAmmoMap[WeaponType] + AmmoAmount, 0, MaxCarriedAmmo);

		UpdateCarriedAmmo();
	}
	if (EquippedWeapon && EquippedWeapon->IsEmpty() && EquippedWeapon->GetWeaponType() == WeaponType) {
		Reload();

	}




}

void UCombatComponent::Server_ThrowGrenade_Implementation() {

	CombatState = E_CombatState::ECS_ThrowingGrenade;

	if (Character) {

		Character->PlayThrowGrenadeMonatge();
		AttachActorToLeftHand(EquippedWeapon);
		ShowAttachedGrenade(1);
	}

}

void UCombatComponent::Server_Reload_Implementation() {
	
	if (Character == nullptr||EquippedWeapon==nullptr) {
		return;
	}
	
	CombatState = E_CombatState::ECS_Reloading;
	if(!Character->IsLocallyControlled()) HandleReload();

}

void UCombatComponent::InterpFOV(float DeltaTime) {
	if (EquippedWeapon == nullptr)return;

	if (bAiming) {
		CurrentFOV = FMath::FInterpTo(CurrentFOV, EquippedWeapon->GetZoomedFOV(), DeltaTime, EquippedWeapon->GetZoomInterpSpeed());

		
	}
	else {
		CurrentFOV = FMath::FInterpTo(CurrentFOV, DefaultFOV, DeltaTime, ZoomInterpSpeed);

	}

	if (Character && Character->GetFollowCamera()) {
		Character->GetFollowCamera()->SetFieldOfView(CurrentFOV);


	}






}

void UCombatComponent::StartFireTimer() {

	if (EquippedWeapon == nullptr||Character==nullptr) {
		return;
	}

	Character->GetWorldTimerManager().SetTimer(FireTimer, this, &UCombatComponent::FireTimerFinished,EquippedWeapon->FireDelay);






}

void UCombatComponent::FireTimerFinished() {
	if (EquippedWeapon == nullptr ) {
		return;
	}
	
	bCanFire = 1;
	if (bFireButtonPressed && EquippedWeapon->bAutomatic) {
		Fire();
	}
	ReloadEmptyWeapon();



}

bool UCombatComponent::CanFire() {
	if (EquippedWeapon == nullptr)return 0;
	if (bLocallyReloading)return 0;
	return !EquippedWeapon->IsEmpty() && bCanFire && CombatState == E_CombatState::ECS_UnOccupied;

}

void UCombatComponent::OnRep_CarriedAmmo() {

	Controller = Controller == nullptr ? Cast<ABlasterPlayerController>(Character->Controller) : Controller;
	if (Controller) {

		Controller->SetHUDCarriedAmmo(CarriedAmmo);

	}

}

void UCombatComponent::InitializeCarriedAmmo() {
	CarriedAmmoMap.Emplace(E_WeaponType::EWT_AssaultRifle, StartingARAmmo);
	CarriedAmmoMap.Emplace(E_WeaponType::EWT_RocketLauncher, StartingRocketAmmo);
	CarriedAmmoMap.Emplace(E_WeaponType::EWT_Pistol, StartingPistolAmmo);
	CarriedAmmoMap.Emplace(E_WeaponType::EWT_SubmachineGun, StartingSMGAmmo);
	CarriedAmmoMap.Emplace(E_WeaponType::EWT_ShotGun, StartingShotGunAmmo);
	CarriedAmmoMap.Emplace(E_WeaponType::EWT_SniperRifle, StartingSniperAmmo);
	CarriedAmmoMap.Emplace(E_WeaponType::EWT_GrenadeLauncher, StartingGrenadeLauncherAmmo);


}

void UCombatComponent::OnRep_CombatState() {

	switch (CombatState) {
	case E_CombatState::ECS_UnOccupied:
		if (bFireButtonPressed) {
			Fire();
		}
		break;
	case E_CombatState::ECS_Reloading:
		if (Character && !Character->IsLocallyControlled()) HandleReload();
		break;
	case E_CombatState::ECS_ThrowingGrenade:
		if (Character && !Character->IsLocallyControlled()) {

			Character->PlayThrowGrenadeMonatge();
			AttachActorToLeftHand(EquippedWeapon);
			ShowAttachedGrenade(1);
		}
		break;
	case E_CombatState::ECS_SwappingWeapons:
		
		if (Character && !Character->IsLocallyControlled()) {

			Character->PlaySwapWeaponMonatge();
		}


		break;
	case E_CombatState::ECS_MAX:
		break;
	default:
		break;
	}



}

void UCombatComponent::UpdateAmmoValues() {
	if (Character == nullptr || EquippedWeapon == nullptr) {
		return;
	}


	int32 ReloadAmount = AmountToReload();
	if (CarriedAmmoMap.Contains(EquippedWeapon->GetWeaponType())) {
		CarriedAmmoMap[EquippedWeapon->GetWeaponType()] -= ReloadAmount;
		CarriedAmmo = CarriedAmmoMap[EquippedWeapon->GetWeaponType()];


	}
	Controller = Controller == nullptr ? Cast<ABlasterPlayerController>(Character->Controller) : Controller;
	if (Controller) {

		Controller->SetHUDCarriedAmmo(CarriedAmmo);

	}

	EquippedWeapon->AddAmmo(ReloadAmount);




}

bool UCombatComponent::ShouldSwapWeapons() {

	return (EquippedWeapon != nullptr && SecondaryWeapon != nullptr);
}









// Called every frame
void UCombatComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);


	
	if (Character && Character->IsLocallyControlled()) {

		
		FHitResult HitResult;
		TraceUnderCrosshairs(HitResult);

		HitTarget = HitResult.ImpactPoint;

		SetHUDCrosshairs(DeltaTime);
		InterpFOV( DeltaTime);
	}

	
	// ...
}

void UCombatComponent::EquipWeapon(AWeapon* WeaponToEquip) {

	if (Character == nullptr || WeaponToEquip == nullptr)return;
	if (CombatState != E_CombatState::ECS_UnOccupied)return;

	if (EquippedWeapon != nullptr && SecondaryWeapon == nullptr) {
		EquipSecondaryWeapon(WeaponToEquip);

	}
	else {
		EquipPrimaryWeapon(WeaponToEquip);
	}
	
	



	Character->GetCharacterMovement()->bOrientRotationToMovement = 0;
	Character->bUseControllerRotationYaw = 1;


}

void UCombatComponent::SwapWeapon() {
	if (CombatState != E_CombatState::ECS_UnOccupied || Character == nullptr)return;

	Character->PlaySwapWeaponMonatge();
	Character->bFinishedSwapping = 0;
	CombatState = E_CombatState::ECS_SwappingWeapons;

	AWeapon* TempWeapon = EquippedWeapon;
	EquippedWeapon = SecondaryWeapon;
	SecondaryWeapon = TempWeapon;

	




}

void UCombatComponent::Reload() {

	if (CarriedAmmo > 0 && CombatState == E_CombatState::ECS_UnOccupied && EquippedWeapon && !EquippedWeapon->GetIsFull() && !bLocallyReloading) {
		
		HandleReload();
		bLocallyReloading = 1;
		Server_Reload();
	}



}

void UCombatComponent::FinishReloading() {
	if (Character == nullptr)return;
	bLocallyReloading = 0;
	if (Character->HasAuthority()) {
		CombatState = E_CombatState::ECS_UnOccupied;
		UpdateAmmoValues();


	}
	if (bFireButtonPressed) {
		Fire();
	}
	

}

void UCombatComponent::FinishSwap() {

	if (Character && Character->HasAuthority()) {
		CombatState = E_CombatState::ECS_UnOccupied;

	}

	if (Character)Character->bFinishedSwapping = 1;
	//if (Character&& Character->IsLocallyControlled() && !Character->bIsFirstSwapDone)Character->bIsFirstSwapDone = 1;

}

void UCombatComponent::FinishSwapAttachWeapons() {
	EquippedWeapon->SetWeaponState(E_WeaponState::EWS_Equipped);
	AttachActorToRightHand(EquippedWeapon);
	EquippedWeapon->SetHUDAmmo();
	UpdateCarriedAmmo();
	PlayEquipWeaponSound(EquippedWeapon);

	SecondaryWeapon->SetWeaponState(E_WeaponState::EWS_EquippedSecondary);
	AttachActorToBackpack(SecondaryWeapon);
	

}





