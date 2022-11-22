// Fill out your copyright notice in the Description page of Project Settings.


#include "Weapons/Weapon.h"
#include "Components/SphereComponent.h"
#include "Components/WidgetComponent.h"
#include "Character/BlasterCharacter.h"
#include "Net/UnrealNetwork.h"
#include "Animation/AnimationAsset.h"
#include "Weapons/Casing.h"
#include "Engine/SkeletalMeshSocket.h"
#include "PlayerControllers/BlasterPlayerController.h"
#include "Kismet/KismetMathLibrary.h"







// Sets default values
AWeapon::AWeapon()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = 0;
	bReplicates = 1;
	SetReplicateMovement(1);

	WeaponMesh = CreateDefaultSubobject<USkeletalMeshComponent>("WeaponMesh");
	WeaponMesh->SetupAttachment(RootComponent);
	WeaponMesh->SetCollisionResponseToAllChannels(ECollisionResponse::ECR_Block);
	WeaponMesh->SetCollisionResponseToChannel(ECollisionChannel::ECC_Pawn, ECollisionResponse::ECR_Ignore);
	WeaponMesh->SetCollisionResponseToChannel(ECollisionChannel::ECC_Camera, ECollisionResponse::ECR_Ignore);
	WeaponMesh->SetCollisionEnabled(ECollisionEnabled::NoCollision);
	WeaponMesh->SetCustomDepthStencilValue(CUSTOM_DEPTH_PURPLE);
	WeaponMesh->MarkRenderStateDirty();
	EnableCustomDepth(1);
	SetRootComponent(WeaponMesh);

	AreaSphere = CreateDefaultSubobject<USphereComponent>("AreaSphere");
	AreaSphere->SetupAttachment(RootComponent);
	AreaSphere->SetSphereRadius(180.f);
	AreaSphere->SetCollisionResponseToAllChannels(ECollisionResponse::ECR_Ignore);
	AreaSphere->SetCollisionEnabled(ECollisionEnabled::NoCollision);

	PickupWidget = CreateDefaultSubobject<UWidgetComponent>("PickupWidget");
	PickupWidget->SetupAttachment(RootComponent);





}



// Called when the game starts or when spawned
void AWeapon::BeginPlay()
{
	Super::BeginPlay();
	

	AreaSphere->SetCollisionEnabled(ECollisionEnabled::QueryAndPhysics);
	AreaSphere->SetCollisionResponseToChannel(ECollisionChannel::ECC_Pawn, ECollisionResponse::ECR_Overlap);

	AreaSphere->OnComponentBeginOverlap.AddDynamic(this, &AWeapon::OnSphereOverlap);
	AreaSphere->OnComponentEndOverlap.AddDynamic(this, &AWeapon::OnSphereEndOverlap);



	if (PickupWidget) {
		PickupWidget->SetVisibility(0);
	}







}

FVector AWeapon::TraceEndWithScatter(const FVector& HitTarget) {

	const USkeletalMeshSocket* MuzzleFlashSocket = GetWeaponMesh()->GetSocketByName("MuzzleFlash");
	if (MuzzleFlashSocket == nullptr)return FVector::ZeroVector;


	const FTransform SocketTransform = MuzzleFlashSocket->GetSocketTransform(GetWeaponMesh());
	const FVector TraceStart = SocketTransform.GetLocation();

	const FVector ToTargetNormalized = (HitTarget - TraceStart).GetSafeNormal();
	const FVector SphereCenter = TraceStart + ToTargetNormalized * DistanceToSphere;
	const FVector RandVec = UKismetMathLibrary::RandomUnitVector() * FMath::FRandRange(0.f, SphereRadius);
	const FVector EndLoc = SphereCenter + RandVec;
	const FVector ToEndLoc = EndLoc - TraceStart;




	//DrawDebugSphere(GetWorld(), SphereCenter, SphereRadius, 12, FColor::Cyan, 1);
	//DrawDebugSphere(GetWorld(), EndLoc, 4.f, 12, FColor::Orange, 1);
	//DrawDebugLine(GetWorld(), TraceStart, FVector(TraceStart + ToEndLoc * TRACE_LENGTH / ToEndLoc.Size()), FColor::Cyan, 1);

	return FVector(TraceStart + ToEndLoc * TRACE_LENGTH / ToEndLoc.Size());
}



// Called every frame
void AWeapon::Tick(float DeltaTime) {
	Super::Tick(DeltaTime);

}

void AWeapon::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const {

	Super::GetLifetimeReplicatedProps(OutLifetimeProps);
	DOREPLIFETIME(AWeapon, WeaponState);
	DOREPLIFETIME_CONDITION(AWeapon, bUseServerSideRewind,COND_OwnerOnly);

	

}

void AWeapon::OnSphereOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult) {
	ABlasterCharacter* BlasterCharacterx = Cast<ABlasterCharacter>(OtherActor);

	if (BlasterCharacterx) {
		BlasterCharacterx->SetOverlappingWeapon(this);
	}



}

void AWeapon::OnSphereEndOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex) {
	ABlasterCharacter* BlasterCharacterx = Cast<ABlasterCharacter>(OtherActor);

	if (BlasterCharacterx) {
		BlasterCharacterx->SetOverlappingWeapon(nullptr);
	}


}

void AWeapon::OnRep_Owner() {

	Super::OnRep_Owner();
	if (Owner == nullptr) {

		BlasterCharacter = nullptr;
		BlasterPlayerController = nullptr;
	}
	else {

		BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(Owner) : BlasterCharacter;
		if (BlasterCharacter && BlasterCharacter->GetEquippedWeapon() && BlasterCharacter->GetEquippedWeapon() == this) {

			SetHUDAmmo();

		}

		
	}
	



}






void AWeapon::SpendRound() {

	Ammo = FMath::Clamp(Ammo - 1, 0, MagCapacity);

	SetHUDAmmo();

	if (HasAuthority()) {
		Client_UpdateAmmo(Ammo);

	}
	else {
		++Sequence;
	}




}



void AWeapon::Client_UpdateAmmo_Implementation(int32 ServerAmmo) {//Implementation
	if (HasAuthority())return;
	Ammo = ServerAmmo;
	--Sequence;
	Ammo -= Sequence;
	SetHUDAmmo();


}




void AWeapon::AddAmmo(int32 AmmoToAdd) {
	Ammo = FMath::Clamp(Ammo + AmmoToAdd, 0, MagCapacity);
	SetHUDAmmo();
	Client_AddAmmo(AmmoToAdd);


}

void AWeapon::Client_AddAmmo_Implementation(int32 AmmoToAdd) {
	if (HasAuthority())return;
	Ammo = FMath::Clamp(Ammo + AmmoToAdd, 0, MagCapacity);
	SetHUDAmmo();



}




void AWeapon::SetHUDAmmo() {
	BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(GetOwner()) : BlasterCharacter;

	if (BlasterCharacter) {

		BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(BlasterCharacter->Controller) : BlasterPlayerController;
		if (BlasterPlayerController) {

			BlasterPlayerController->SetHUDWeaponAmmo(Ammo);


		}

	}
}

void AWeapon::EnableCustomDepth(bool bEnable) {
	if (WeaponMesh) {
		WeaponMesh->SetRenderCustomDepth(bEnable);


	}





}






void AWeapon::ShowPickupWidget(bool bShowWidget) {

	if (PickupWidget) {
		PickupWidget->SetVisibility(bShowWidget);
	}
}

void AWeapon::SetWeaponState(E_WeaponState WeaponStatex) {
	WeaponState = WeaponStatex;

	OnWeaponStateSet();


	


}


void AWeapon::OnWeaponStateSet() {

	switch (WeaponState) {
	case E_WeaponState::EWS_Initial:
		break;
	case E_WeaponState::EWS_Equipped:
		
		OnEquipped();

		break;
	case E_WeaponState::EWS_EquippedSecondary:

		OnEquippedSecondary();

		break;
	case E_WeaponState::EWS_Dropped:
		OnDropped();

		break;
	case E_WeaponState::EWS_MAX:
		break;
	default:
		break;
	}






}



void AWeapon::OnPingTooHigh(bool bPingTooHi) {
	bUseServerSideRewind = !bPingTooHi;

}

void AWeapon::OnRep_WeaponState() {
	OnWeaponStateSet();

}




void AWeapon::OnEquipped() {

	ChangePhysicsAssetx1();
	ShowPickupWidget(0);
	AreaSphere->SetCollisionEnabled(ECollisionEnabled::NoCollision);
	WeaponMesh->SetSimulatePhysics(0);
	WeaponMesh->SetEnableGravity(0);
	WeaponMesh->SetCollisionEnabled(ECollisionEnabled::NoCollision);
	if (WeaponType == E_WeaponType::EWT_SubmachineGun) {
		WeaponMesh->SetCollisionEnabled(ECollisionEnabled::QueryAndPhysics);
		WeaponMesh->SetEnableGravity(1);
		WeaponMesh->SetCollisionResponseToAllChannels(ECollisionResponse::ECR_Ignore);


	}

	EnableCustomDepth(0);


	BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(GetOwner()) : BlasterCharacter;

	if (BlasterCharacter&&bUseServerSideRewind) {

		BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(BlasterCharacter->Controller) : BlasterPlayerController;
		if (BlasterPlayerController && HasAuthority() && !BlasterPlayerController->HighPingDelegate.IsBound()) {
			BlasterPlayerController->HighPingDelegate.AddDynamic(this, &AWeapon::OnPingTooHigh);
			


		}

	}






}

void AWeapon::OnDropped() {

	if (HasAuthority()) {
		AreaSphere->SetCollisionEnabled(ECollisionEnabled::QueryOnly);

	}
	ChangePhysicsAssetx();
	WeaponMesh->SetSimulatePhysics(1);
	WeaponMesh->SetEnableGravity(1);
	WeaponMesh->SetCollisionEnabled(ECollisionEnabled::QueryAndPhysics);

	WeaponMesh->SetCollisionResponseToAllChannels(ECollisionResponse::ECR_Block);
	WeaponMesh->SetCollisionResponseToChannel(ECollisionChannel::ECC_Pawn, ECollisionResponse::ECR_Ignore);
	WeaponMesh->SetCollisionResponseToChannel(ECollisionChannel::ECC_Camera, ECollisionResponse::ECR_Ignore);

	WeaponMesh->SetCustomDepthStencilValue(CUSTOM_DEPTH_PURPLE);
	WeaponMesh->MarkRenderStateDirty();
	EnableCustomDepth(1);


	BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(GetOwner()) : BlasterCharacter;

	if (BlasterCharacter && bUseServerSideRewind) {

		BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(BlasterCharacter->Controller) : BlasterPlayerController;
		if (BlasterPlayerController && HasAuthority() && BlasterPlayerController->HighPingDelegate.IsBound()) {
			BlasterPlayerController->HighPingDelegate.RemoveDynamic(this, &AWeapon::OnPingTooHigh);



		}

	}


}

void AWeapon::OnEquippedSecondary() {


	ChangePhysicsAssetx1();
	ShowPickupWidget(0);
	AreaSphere->SetCollisionEnabled(ECollisionEnabled::NoCollision);
	WeaponMesh->SetSimulatePhysics(0);
	WeaponMesh->SetEnableGravity(0);
	WeaponMesh->SetCollisionEnabled(ECollisionEnabled::NoCollision);
	if (WeaponType == E_WeaponType::EWT_SubmachineGun) {
		WeaponMesh->SetCollisionEnabled(ECollisionEnabled::QueryAndPhysics);
		WeaponMesh->SetEnableGravity(1);
		WeaponMesh->SetCollisionResponseToAllChannels(ECollisionResponse::ECR_Ignore);


	}
	EnableCustomDepth(1);

	if (GetWeaponMesh()) {

		GetWeaponMesh()->SetCustomDepthStencilValue(CUSTOM_DEPTH_BLUE);
		GetWeaponMesh()->MarkRenderStateDirty();

	}

	BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(GetOwner()) : BlasterCharacter;

	if (BlasterCharacter && bUseServerSideRewind) {

		BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(BlasterCharacter->Controller) : BlasterPlayerController;
		if (BlasterPlayerController && HasAuthority() && BlasterPlayerController->HighPingDelegate.IsBound()) {
			BlasterPlayerController->HighPingDelegate.RemoveDynamic(this, &AWeapon::OnPingTooHigh);



		}

	}


}








void AWeapon::Fire(const FVector& HitTarget) {
	if (FireAnimation) {
		WeaponMesh->PlayAnimation(FireAnimation, 0);
	}
	if (CasingClass) {


		const USkeletalMeshSocket* AmmoEjectSocket = GetWeaponMesh()->GetSocketByName("AmmoEject");
		if (AmmoEjectSocket) {

			FTransform SocketTransform = AmmoEjectSocket->GetSocketTransform(GetWeaponMesh());


			
			UWorld* World = GetWorld();
			if (World) {
				World->SpawnActor<ACasing>(CasingClass, SocketTransform.GetLocation(), SocketTransform.GetRotation().Rotator());
			}



		}








	}
	SpendRound();
	
	

}

void AWeapon::Dropped() {

	SetWeaponState(E_WeaponState::EWS_Dropped);

	FDetachmentTransformRules DetachRules(EDetachmentRule::KeepWorld, 1);
	WeaponMesh->DetachFromComponent(DetachRules);

	SetOwner(nullptr);

	BlasterCharacter = nullptr;
	BlasterPlayerController = nullptr;

}

bool AWeapon::IsEmpty() {
	return Ammo <= 0;

	
}





