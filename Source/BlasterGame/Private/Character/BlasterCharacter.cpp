// Fill out your copyright notice in the Description page of Project Settings.


#include "Character/BlasterCharacter.h"
#include "GameFramework/SpringArmComponent.h"
#include "Camera/CameraComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Components/WidgetComponent.h"
#include "Net/UnrealNetwork.h"
#include "Weapons/Weapon.h"
#include "BlasterComponents/CombatComponent.h"
#include "BlasterComponents/BuffComponent.h"
#include "Components/CapsuleComponent.h"
#include "Kismet/KismetMathLibrary.h"
#include "Character/BlasterAnimInstance.h"
#include "PlayerControllers/BlasterPlayerController.h"
#include "GameModes/BlasterGameMode.h"
#include "TimerManager.h"
#include "Kismet/GameplayStatics.h"
#include "Sound/SoundCue.h"
#include "Particles/ParticleSystemComponent.h"
#include "PlayerStates/BlasterPlayerState.h"
#include "Components/BoxComponent.h"
#include "BlasterComponents/LagCompensationComponent.h"






// Sets default values
ABlasterCharacter::ABlasterCharacter()
{
 	// Set this character to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	SpawnCollisionHandlingMethod = ESpawnActorCollisionHandlingMethod::AdjustIfPossibleButAlwaysSpawn;

	CameraBoom = CreateDefaultSubobject<USpringArmComponent>("CameraBoom");
	CameraBoom->SetupAttachment(GetMesh());
	CameraBoom->TargetArmLength = 600.f;
	CameraBoom->bUsePawnControlRotation = 1;


	FollowCamera = CreateDefaultSubobject<UCameraComponent>("FollowCamera");
	FollowCamera->SetupAttachment(CameraBoom, USpringArmComponent::SocketName);
	FollowCamera->bUsePawnControlRotation = 0;

	bUseControllerRotationYaw = 0;

	GetCharacterMovement()->bOrientRotationToMovement = 1;

	OverheadWidget = CreateDefaultSubobject<UWidgetComponent>("OverheadWidget");
	OverheadWidget->SetupAttachment(RootComponent);

	Combat = CreateDefaultSubobject<UCombatComponent>("CombatComponent");
	Combat->SetIsReplicated(1);

	Buff = CreateDefaultSubobject<UBuffComponent>("BuffComponent");
	Buff->SetIsReplicated(1);


	GetCharacterMovement()->NavAgentProps.bCanCrouch = 1;
	GetCapsuleComponent()->SetCollisionResponseToChannel(ECollisionChannel::ECC_Camera, ECollisionResponse::ECR_Ignore);
	GetMesh()->SetCollisionObjectType(ECC_SkeletalMesh);
	GetMesh()->SetCollisionResponseToChannel(ECollisionChannel::ECC_Camera, ECollisionResponse::ECR_Ignore);
	GetMesh()->SetCollisionResponseToChannel(ECollisionChannel::ECC_Visibility, ECollisionResponse::ECR_Block);


	NetUpdateFrequency = 66.f;
	MinNetUpdateFrequency = 33.f;

	GetCharacterMovement()->RotationRate = FRotator(0.f, 850.f, 0.f);

	DissolveTimeline = CreateDefaultSubobject<UTimelineComponent>("DissolveTimelineComp");


	AttachedGrenade = CreateDefaultSubobject<UStaticMeshComponent>("AttachedGrenade");
	AttachedGrenade->SetupAttachment(GetMesh(), "GrenadeSocket");
	AttachedGrenade->SetCollisionEnabled(ECollisionEnabled::NoCollision);

	LagCompensation = CreateDefaultSubobject<ULagCompensationComponent>("LagCompensation");


	//HitBoxes for ServerSideRewind


	

	head = CreateDefaultSubobject<UBoxComponent>(TEXT("head"));
	head->SetupAttachment(GetMesh(), FName("head"));
	HitCollisionBoxes.Add(FName("head"), head);

	pelvis = CreateDefaultSubobject<UBoxComponent>(TEXT("pelvis"));
	pelvis->SetupAttachment(GetMesh(), FName("pelvis"));
	HitCollisionBoxes.Add(FName("pelvis"), pelvis);

	spine_02 = CreateDefaultSubobject<UBoxComponent>(TEXT("spine_02"));
	spine_02->SetupAttachment(GetMesh(), FName("spine_02"));
	HitCollisionBoxes.Add(FName("spine_02"), spine_02);

	spine_03 = CreateDefaultSubobject<UBoxComponent>(TEXT("spine_03"));
	spine_03->SetupAttachment(GetMesh(), FName("spine_03"));
	HitCollisionBoxes.Add(FName("spine_03"), spine_03);

	upperarm_l = CreateDefaultSubobject<UBoxComponent>(TEXT("upperarm_l"));
	upperarm_l->SetupAttachment(GetMesh(), FName("upperarm_l"));
	HitCollisionBoxes.Add(FName("upperarm_l"), upperarm_l);

	upperarm_r = CreateDefaultSubobject<UBoxComponent>(TEXT("upperarm_r"));
	upperarm_r->SetupAttachment(GetMesh(), FName("upperarm_r"));
	HitCollisionBoxes.Add(FName("upperarm_r"), upperarm_r);

	lowerarm_l = CreateDefaultSubobject<UBoxComponent>(TEXT("lowerarm_l"));
	lowerarm_l->SetupAttachment(GetMesh(), FName("lowerarm_l"));
	HitCollisionBoxes.Add(FName("lowerarm_l"), lowerarm_l);

	lowerarm_r = CreateDefaultSubobject<UBoxComponent>(TEXT("lowerarm_r"));
	lowerarm_r->SetupAttachment(GetMesh(), FName("lowerarm_r"));
	HitCollisionBoxes.Add(FName("lowerarm_r"), lowerarm_r);

	hand_l = CreateDefaultSubobject<UBoxComponent>(TEXT("hand_l"));
	hand_l->SetupAttachment(GetMesh(), FName("hand_l"));
	HitCollisionBoxes.Add(FName("hand_l"), hand_l);

	hand_r = CreateDefaultSubobject<UBoxComponent>(TEXT("hand_r"));
	hand_r->SetupAttachment(GetMesh(), FName("hand_r"));
	HitCollisionBoxes.Add(FName("hand_r"), hand_r);

	blanket = CreateDefaultSubobject<UBoxComponent>(TEXT("blanket"));
	blanket->SetupAttachment(GetMesh(), FName("backpack"));
	HitCollisionBoxes.Add(FName("blanket"), blanket);

	backpack = CreateDefaultSubobject<UBoxComponent>(TEXT("backpack"));
	backpack->SetupAttachment(GetMesh(), FName("backpack"));
	HitCollisionBoxes.Add(FName("backpack"), backpack);

	thigh_l = CreateDefaultSubobject<UBoxComponent>(TEXT("thigh_l"));
	thigh_l->SetupAttachment(GetMesh(), FName("thigh_l"));
	HitCollisionBoxes.Add(FName("thigh_l"), thigh_l);

	thigh_r = CreateDefaultSubobject<UBoxComponent>(TEXT("thigh_r"));
	thigh_r->SetupAttachment(GetMesh(), FName("thigh_r"));
	HitCollisionBoxes.Add(FName("thigh_r"), thigh_r);

	calf_l = CreateDefaultSubobject<UBoxComponent>(TEXT("calf_l"));
	calf_l->SetupAttachment(GetMesh(), FName("calf_l"));
	HitCollisionBoxes.Add(FName("calf_l"), calf_l);

	calf_r = CreateDefaultSubobject<UBoxComponent>(TEXT("calf_r"));
	calf_r->SetupAttachment(GetMesh(), FName("calf_r"));
	HitCollisionBoxes.Add(FName("calf_r"), calf_r);

	foot_l = CreateDefaultSubobject<UBoxComponent>(TEXT("foot_l"));
	foot_l->SetupAttachment(GetMesh(), FName("foot_l"));
	HitCollisionBoxes.Add(FName("foot_l"), foot_l);

	foot_r = CreateDefaultSubobject<UBoxComponent>(TEXT("foot_r"));
	foot_r->SetupAttachment(GetMesh(), FName("foot_r"));
	HitCollisionBoxes.Add(FName("foot_r"), foot_r);

	for (auto Box : HitCollisionBoxes) {
		if (Box.Value) {
			Box.Value->SetCollisionObjectType(ECC_HitBox);
			Box.Value->SetCollisionResponseToAllChannels(ECollisionResponse::ECR_Ignore);
			Box.Value->SetCollisionResponseToChannel(ECC_HitBox, ECollisionResponse::ECR_Block);
			Box.Value->SetCollisionEnabled(ECollisionEnabled::NoCollision);
		}
	}










}

void ABlasterCharacter::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const {

	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME_CONDITION(ABlasterCharacter, OverlappingWeapon, COND_OwnerOnly);

	DOREPLIFETIME(ABlasterCharacter, Health);
	DOREPLIFETIME(ABlasterCharacter, Sheild);

	DOREPLIFETIME(ABlasterCharacter, bDisableGameplay);




}

void ABlasterCharacter::PostInitializeComponents() {
	Super::PostInitializeComponents();

	if (Combat) {
		Combat->Character = this;
	}
	if (Buff) {
		Buff->Character = this;

		Buff->SetInitialSpeeds(GetCharacterMovement()->MaxWalkSpeed, GetCharacterMovement()->MaxWalkSpeedCrouched);
	
		Buff->SetInitialJumpVelocity(GetCharacterMovement()->JumpZVelocity);
	}

	if (LagCompensation) {
		LagCompensation->Character = this;
		if (Controller) {
			LagCompensation->Controller = Cast<ABlasterPlayerController>(Controller);

		}

	}





}

// Called to bind functionality to input
void ABlasterCharacter::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent) {
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	PlayerInputComponent->BindAction("Jump", IE_Pressed, this, &ABlasterCharacter::Jump);
	
	PlayerInputComponent->BindAxis("Move Forward / Backward", this, &ABlasterCharacter::MoveForward);
	PlayerInputComponent->BindAxis("Move Right / Left", this, &ABlasterCharacter::MoveRight);

	PlayerInputComponent->BindAxis("Turn Right / Left Mouse", this, &ABlasterCharacter::Turn);
	PlayerInputComponent->BindAxis("Look Up / Down Mouse", this, &ABlasterCharacter::LookUp);

	PlayerInputComponent->BindAction("Equip", IE_Pressed, this, &ABlasterCharacter::EquipButtonPressed);

	PlayerInputComponent->BindAction("Crouch", IE_Pressed, this, &ABlasterCharacter::CrouchButtonPressed);

	PlayerInputComponent->BindAction("Aim", IE_Pressed, this, &ABlasterCharacter::AimButtonPressed);
	PlayerInputComponent->BindAction("Aim", IE_Released, this, &ABlasterCharacter::AimButtonReleased);

	PlayerInputComponent->BindAction("Fire", IE_Pressed, this, &ABlasterCharacter::FireButtonPressed);
	PlayerInputComponent->BindAction("Fire", IE_Released, this, &ABlasterCharacter::FireButtonReleased);

	PlayerInputComponent->BindAction("Reload", IE_Pressed, this, &ABlasterCharacter::ReloadButtonPressed);

	PlayerInputComponent->BindAction("ThrowGrenade", IE_Pressed, this, &ABlasterCharacter::ThrowButtonPressed);



}








// Called when the game starts or when spawned
void ABlasterCharacter::BeginPlay()
{
	Super::BeginPlay();
	
	SpawnDefaultWeapon();
	UpdateHUDAmmo();

	Health = MaxHealth;
	Sheild = MaxSheild;

	UpdateHUDHealth();
	UpdateHUDSheild();

	if (HasAuthority()) {
		OnTakeAnyDamage.AddDynamic(this, &ABlasterCharacter::ReceiveDamage);
	}

	if (AttachedGrenade) {
		AttachedGrenade->SetVisibility(0);


	}


}

void ABlasterCharacter::UpdateHUDHealth() {
	BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(Controller) : BlasterPlayerController;
	if (BlasterPlayerController) {
		BlasterPlayerController->SetHUDHealth(Health, MaxHealth);
	}
}

void ABlasterCharacter::UpdateHUDSheild() {
	BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(Controller) : BlasterPlayerController;
	if (BlasterPlayerController) {
		BlasterPlayerController->SetHUDSheild(Sheild, MaxSheild);
	}



}

void ABlasterCharacter::UpdateHUDAmmo() {

	BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(Controller) : BlasterPlayerController;
	if (BlasterPlayerController && Combat && Combat->EquippedWeapon) {
		BlasterPlayerController->SetHUDCarriedAmmo(Combat->CarriedAmmo);
		BlasterPlayerController->SetHUDWeaponAmmo(Combat->EquippedWeapon->GetAmmo());

	}




}

void ABlasterCharacter::SpawnDefaultWeapon() {

	ABlasterGameMode* BlasterGameMode = Cast<ABlasterGameMode>(UGameplayStatics::GetGameMode(this));
	UWorld* World = GetWorld();
	if (BlasterGameMode && World && !bElimmed && DefaultWeaponClass) {
		AWeapon* StartingWeapon= World->SpawnActor<AWeapon>(DefaultWeaponClass);
		if (Combat && StartingWeapon) {
			StartingWeapon->bDestroyWeapon = 1;
			Combat->EquipWeapon(StartingWeapon);


		}

	}


}



// Called every frame
void ABlasterCharacter::Tick(float DeltaTime) {
	Super::Tick(DeltaTime);

	RotateInPlace(DeltaTime);
	
	
	HideCharacterIfCameraClose();
	PollInit();

	StartingPollForHealth(DeltaTime);



}



void ABlasterCharacter::MoveForward(float Value) {

	if (bDisableGameplay)return;

	if (Controller != nullptr && Value != 0.f) {
		const FRotator YawRotation = FRotator(0.f, Controller->GetControlRotation().Yaw, 0.f);
		const FVector Direction(FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X));
		AddMovementInput(Direction, Value);


	}


}

void ABlasterCharacter::MoveRight(float Value) {
	if (bDisableGameplay)return;

	if (Controller != nullptr && Value != 0.f) {
		const FRotator YawRotation = FRotator(0.f, Controller->GetControlRotation().Yaw, 0.f);
		const FVector Direction(FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y));
		AddMovementInput(Direction, Value);


	}

}

void ABlasterCharacter::Turn(float Value) {

	AddControllerYawInput(Value * .7f);

}

void ABlasterCharacter::LookUp(float Value) {
	AddControllerPitchInput(Value * .7f);

}

void ABlasterCharacter::EquipButtonPressed() {

	if (bDisableGameplay)return;



	if (Combat) {
		if (Combat->CombatState == E_CombatState::ECS_UnOccupied) Server_EquipButtonPressed();
		if (Combat->ShouldSwapWeapons() && !HasAuthority() && Combat->CombatState == E_CombatState::ECS_UnOccupied && OverlappingWeapon == nullptr) {
			PlaySwapWeaponMonatge();
			Combat->CombatState = E_CombatState::ECS_SwappingWeapons;
			bFinishedSwapping = 0;

		}

	}




}

void ABlasterCharacter::CrouchButtonPressed() {
	if (bDisableGameplay)return;

	if (bIsCrouched) {
		UnCrouch();
	}
	else {
		Crouch();
	}
	


}

void ABlasterCharacter::ReloadButtonPressed() {
	if (bDisableGameplay)return;

	if (Combat) {
		Combat->Reload();
	}



}

void ABlasterCharacter::AimButtonPressed() {

	if (bDisableGameplay)return;

	if (Combat) {
		Combat->SetAiming(1);
	}


}

void ABlasterCharacter::AimButtonReleased() {

	if (bDisableGameplay)return;

	if (Combat) {
		Combat->SetAiming(0);
	}




}

void ABlasterCharacter::AimOffset(float DeltaTime) {
	if (Combat && Combat->EquippedWeapon == nullptr)return;

	
	float Speed = CalculateSpeed();

	bool bIsInAir = GetCharacterMovement()->IsFalling();

	if (Speed == 0.f && !bIsInAir) {

		bRotateRootBone = 1;
		FRotator CurrentAimRotation = FRotator(0.f, GetBaseAimRotation().Yaw, 0.f);
		FRotator DeltaAimRotation = UKismetMathLibrary::NormalizedDeltaRotator( CurrentAimRotation,StartingAimRotation);
		AO_Yaw = DeltaAimRotation.Yaw;
		if (TurningInPlace == E_TurningInPlace::ETIP_NotTurning) {
			InterpAO_Yaw = AO_Yaw;
		}

		bUseControllerRotationYaw = 1;
		TurnInPlace(DeltaTime);
	}

	if (Speed > 0.f || bIsInAir) {
		bRotateRootBone = 0;
		StartingAimRotation = FRotator(0.f, GetBaseAimRotation().Yaw, 0.f);
		AO_Yaw = 0.f;
		bUseControllerRotationYaw = 1;
		TurningInPlace = E_TurningInPlace::ETIP_NotTurning;
	}


	CalculateAO_Pitch();



}

void ABlasterCharacter::CalculateAO_Pitch() {
	AO_Pitch = GetBaseAimRotation().Pitch;
	if (AO_Pitch > 90.f && !IsLocallyControlled()) {

		FVector2D InRange(270.f, 360.f);
		FVector2D OutRange(-90.f, 0.f);
		AO_Pitch = FMath::GetMappedRangeValueClamped(InRange, OutRange, AO_Pitch);


	}
}

void ABlasterCharacter::SimProxiesTurn() {

	if (Combat == nullptr || Combat->EquippedWeapon == nullptr) {
		return;
	}
	bRotateRootBone = 0;
	float Speed = CalculateSpeed();
	if (Speed > 0.f) {
		TurningInPlace = E_TurningInPlace::ETIP_NotTurning;
		return;
	}
	
	

	ProxyRotationLastFrame = ProxyRotation;
	ProxyRotation = GetActorRotation();
	ProxyYaw= UKismetMathLibrary::NormalizedDeltaRotator(ProxyRotation, ProxyRotationLastFrame).Yaw;

	//UE_LOG(LogTemp, Warning, TEXT("ProxyYaw %f"), ProxyYaw);

	if (FMath::Abs(ProxyYaw) > TurnThreshold) {
		if (ProxyYaw > TurnThreshold) {
			TurningInPlace = E_TurningInPlace::ETIP_Right;
		}
		else if (ProxyYaw < -TurnThreshold) {
			TurningInPlace = E_TurningInPlace::ETIP_Left;
		}
		else {
			TurningInPlace = E_TurningInPlace::ETIP_NotTurning;
		}
		return;
	}
	TurningInPlace = E_TurningInPlace::ETIP_NotTurning;


}

void ABlasterCharacter::FireButtonPressed() {
	if (bDisableGameplay)return;

	if (Combat) {
		Combat->FireButtonPressed(1);
	}

}

void ABlasterCharacter::FireButtonReleased() {
	if (bDisableGameplay)return;

	if (Combat) {
		Combat->FireButtonPressed(0);
	}



}

void ABlasterCharacter::ThrowButtonPressed() {
	if (Combat) {
		Combat->ThrowGrenade();


	}



}

void ABlasterCharacter::ReceiveDamage(AActor* DamagedActor, float Damage, const UDamageType* DamageType, AController* InstigatedBy, AActor* DamageCauser) {

	if (bElimmed)return;

	float DamageToHealth = Damage;

	if (Sheild > 0.f) {
		if (Sheild >= Damage) {
			Sheild = FMath::Clamp(Sheild - Damage, 0.f, MaxSheild);
			DamageToHealth = 0.f;


		}
		else {
			Sheild = 0.f;
			DamageToHealth = FMath::Clamp(DamageToHealth - Sheild, 0.f, Damage);
		}


	}

	Health = FMath::Clamp(Health - DamageToHealth, 0.f, MaxHealth);
	UpdateHUDHealth();
	UpdateHUDSheild();

	PlayHitReactMonatge();

	if (Health == 0.f) {
		ABlasterGameMode* BlasterGameMode = GetWorld()->GetAuthGameMode<ABlasterGameMode>();
		if (BlasterGameMode) {
			BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(Controller) : BlasterPlayerController;
			ABlasterPlayerController* AttackerController = Cast<ABlasterPlayerController>(InstigatedBy);
			BlasterGameMode->PlayerEliminated(this, BlasterPlayerController, AttackerController);
		}
	}

	




}

void ABlasterCharacter::PollInit() {
	if (BlasterPlayerState == nullptr) {
		BlasterPlayerState = GetPlayerState<ABlasterPlayerState>();
		if (BlasterPlayerState) {
			BlasterPlayerState->AddToScore(0.f);
			BlasterPlayerState->AddToDefeats(0);

		}

	}




}

void ABlasterCharacter::RotateInPlace(float DeltaTime) {


	if (bDisableGameplay) {

		bUseControllerRotationYaw = 0;
		TurningInPlace = E_TurningInPlace::ETIP_NotTurning;
		return;

	}


	if (GetLocalRole() > ENetRole::ROLE_SimulatedProxy && IsLocallyControlled()) {
		AimOffset(DeltaTime);
	}
	else {
		TimeSinceLastMovRep += DeltaTime;
		if (TimeSinceLastMovRep > .25f) {
			OnRep_ReplicatedMovement();
		}
		CalculateAO_Pitch();
	}



}

void ABlasterCharacter::OnRep_OverlappingWeapon(AWeapon* LastWeapon) {

	if (OverlappingWeapon) {
		OverlappingWeapon->ShowPickupWidget(1);
	}
	if (LastWeapon) {
		LastWeapon->ShowPickupWidget(0);
	}


}

void ABlasterCharacter::Server_EquipButtonPressed_Implementation() {

	if (Combat) {
		if (OverlappingWeapon) {

			Combat->EquipWeapon(OverlappingWeapon);

		}
		else if (Combat->ShouldSwapWeapons()) {
			Combat->SwapWeapon();

		}


		
	}

}

float ABlasterCharacter::CalculateSpeed() {

	FVector Velocity = GetVelocity();
	Velocity.Z = 0.f;
	
	return Velocity.Size();
}

void ABlasterCharacter::OnRep_Health(float LastHealth) {
	UpdateHUDHealth();
	
	if (Health < LastHealth) {

		PlayHitReactMonatge();

	}
	
	

}

void ABlasterCharacter::OnRep_Sheild(float LastSheild) {

	UpdateHUDSheild();

	if (Sheild < MaxSheild) {
		PlayHitReactMonatge();


	}


}

void ABlasterCharacter::ElimTimerFinished() {
	ABlasterGameMode* BlasterGameMode = GetWorld()->GetAuthGameMode<ABlasterGameMode>();
	if (BlasterGameMode && !bLeftGame) {
		BlasterGameMode->RequestRespawn(this, Controller);
	}
	if (bLeftGame && IsLocallyControlled()) {
		OnLeftGame.Broadcast();


	}

	
	


}

void ABlasterCharacter::Server_LeaveGame_Implementation() {

	ABlasterGameMode* BlasterGameMode = GetWorld()->GetAuthGameMode<ABlasterGameMode>();
	BlasterPlayerState = BlasterPlayerState == nullptr ? GetPlayerState<ABlasterPlayerState>() : BlasterPlayerState;

	if (BlasterGameMode&& BlasterPlayerState) {
		BlasterGameMode->PlayerLeftGame(BlasterPlayerState);
	}




}

void ABlasterCharacter::StartDissolve() {

	DissolveTrack.BindDynamic(this, &ABlasterCharacter::UpdateDissolveMaterial);
	if (DissolveCurve&&DissolveTimeline) {
		DissolveTimeline->AddInterpFloat(DissolveCurve, DissolveTrack);
		DissolveTimeline->Play();
	}


}

void ABlasterCharacter::UpdateDissolveMaterial(float DissolveValue) {

	if (DynamicDissolveMI) {
		DynamicDissolveMI->SetScalarParameterValue("Dissolve", DissolveValue);
	}








}

void ABlasterCharacter::StartingPollForHealth(float DeltaTime) {

	if (HasAuthority()) {
		if (bShouldPoll) {

			PollinTime += DeltaTime;

			if (PollinTime >= 1.f) {
				++PollingInt;
				PollinTime = 0.f;
				UpdateHUDHealth();
				UpdateHUDSheild();
			}
			if (PollingInt >= 2) {
				bShouldPoll = 0;
			}

		}
		


	}


}

void ABlasterCharacter::SetOverlappingWeapon(AWeapon* Weapon) {

	if (OverlappingWeapon) {
		OverlappingWeapon->ShowPickupWidget(0);
	}
	OverlappingWeapon = Weapon;

	if (IsLocallyControlled()) {
		if (OverlappingWeapon) {
			OverlappingWeapon->ShowPickupWidget(1);
		}
		

	}

}

bool ABlasterCharacter::IsWeaponEquipped() {



	return (Combat && Combat->EquippedWeapon);
}

bool ABlasterCharacter::GetIsAiming() {
	return (Combat && Combat->bAiming);
}

AWeapon* ABlasterCharacter::GetEquippedWeapon() {

	if (Combat == nullptr)return nullptr;


	return Combat->EquippedWeapon;
}

void ABlasterCharacter::PlayFireMonatge(bool bAiming) {
	if (Combat == nullptr||Combat->EquippedWeapon==nullptr) {
		return;
	}

	UAnimInstance* AnimInstance = GetMesh()->GetAnimInstance();
	if (AnimInstance && FireWeaponMontage&&FireAimWeaponMontage) {
		if (bAiming) {
			
			AnimInstance->Montage_Play(FireAimWeaponMontage);
			
		}
		else {
			
			AnimInstance->Montage_Play(FireWeaponMontage);
			
		}


	}



}

void ABlasterCharacter::PlayReloadMonatge() {

	if (Combat == nullptr) {
		return;
	}

	UAnimInstance* AnimInstance = GetMesh()->GetAnimInstance();
	if (AnimInstance && ReloadMontage) {

		AnimInstance->Montage_Play(ReloadMontage);
		FName SectionName;

		switch (Combat->EquippedWeapon->GetWeaponType()) {
		case E_WeaponType::EWT_AssaultRifle:

			SectionName = "Rifle";
			break;
		case E_WeaponType::EWT_RocketLauncher:

			SectionName = "Rifle";
			break;
		case E_WeaponType::EWT_Pistol:

			SectionName = "Pistol";
			break;
		case E_WeaponType::EWT_SubmachineGun:

			SectionName = "Pistol";
			break;
		case E_WeaponType::EWT_ShotGun:

			SectionName = "Shotgun";
			break;
		case E_WeaponType::EWT_SniperRifle:

			SectionName = "Sniper";
			break;
		case E_WeaponType::EWT_GrenadeLauncher:

			SectionName = "Rifle";
			break;
		case E_WeaponType::EWT_MAX:
			break;
		default:
			break;
		}



		AnimInstance->Montage_JumpToSection(SectionName);



	}


}

void ABlasterCharacter::PlayElimMontage() {

	if (Combat == nullptr ) {
		return;
	}

	UAnimInstance* AnimInstance = GetMesh()->GetAnimInstance();
	if (AnimInstance && ElimMontage) {

		AnimInstance->Montage_Play(ElimMontage);

	}




}

void ABlasterCharacter::PlayThrowGrenadeMonatge() {

	if (Combat == nullptr) {
		return;
	}

	UAnimInstance* AnimInstance = GetMesh()->GetAnimInstance();
	if (AnimInstance && ThrowGrenadeMontage) {

		AnimInstance->Montage_Play(ThrowGrenadeMontage);

	}



}

void ABlasterCharacter::PlaySwapWeaponMonatge() {

	if (Combat == nullptr) {
		return;
	}

	UAnimInstance* AnimInstance = GetMesh()->GetAnimInstance();
	if (AnimInstance && SwapWeaponMontage) {

		AnimInstance->Montage_Play(SwapWeaponMontage);

	}



}

void ABlasterCharacter::PlayHitReactMonatge() {

	if (Combat == nullptr || Combat->EquippedWeapon == nullptr) {
		return;
	}

	UAnimInstance* AnimInstance = GetMesh()->GetAnimInstance();
	if (AnimInstance && HitReactMontage) {
		AnimInstance->Montage_Play(HitReactMontage);
		FName SectionName="FromLeft";
		int32 RandSection = FMath::RandRange(0,1);
		if (RandSection == 0) {
			SectionName = "FromFront";
		}
		else if (RandSection == 1) {
			SectionName = "FromRight";

		}
		AnimInstance->Montage_JumpToSection(SectionName);
	}


}
























FVector ABlasterCharacter::GetHitTarget() const{
	if (Combat == nullptr)return FVector();


	return Combat->HitTarget;
}

void ABlasterCharacter::Elim(bool bPlayerLeftGame) {

	if (Combat ) {
		if (Combat->EquippedWeapon) {

			if (Combat->EquippedWeapon->bDestroyWeapon) {
				Combat->EquippedWeapon->SetLifeSpan(2.f);


			}
			else {
				Combat->EquippedWeapon->Dropped();
			}

		}

		if (Combat->SecondaryWeapon) {
			if (Combat->SecondaryWeapon->bDestroyWeapon) {
				Combat->SecondaryWeapon->SetLifeSpan(2.f);


			}
			else {
				Combat->SecondaryWeapon->Dropped();
			}


		}

		

	}


	Multicast_Elim(bPlayerLeftGame);

	
	




	
}

E_CombatState ABlasterCharacter::GetCombatState() const {

	if (Combat == nullptr)return E_CombatState::ECS_MAX;
	return Combat->CombatState;


}

bool ABlasterCharacter::GetIsLocallyReloading() {

	if (Combat == nullptr)return 0;
	return Combat->bLocallyReloading;
}

void ABlasterCharacter::Multicast_Elim_Implementation(bool bPlayerLeftGame) {
	
	bLeftGame = bPlayerLeftGame;

	if (BlasterPlayerController) {
		BlasterPlayerController->SetHUDWeaponAmmo(0);
	}
	
	bElimmed = 1;
	PlayElimMontage();


	//StartDissolveEffect


	if (DissolveMI) {
		DynamicDissolveMI = UMaterialInstanceDynamic::Create(DissolveMI, this);
		GetMesh()->SetMaterial(0, DynamicDissolveMI);

		DynamicDissolveMI->SetScalarParameterValue("Dissolve", -.55f);
		DynamicDissolveMI->SetScalarParameterValue("Glow", 200.f);


	}
	StartDissolve();

	//Disable Character Movement

	GetCharacterMovement()->DisableMovement();
	GetCharacterMovement()->StopMovementImmediately();

	bDisableGameplay = 1;
	GetCharacterMovement()->DisableMovement();
	if (Combat) {
		Combat->FireButtonPressed(0);
	}
	//Disable Collission


	GetCapsuleComponent()->SetCollisionEnabled(ECollisionEnabled::NoCollision);
	GetMesh()->SetCollisionEnabled(ECollisionEnabled::NoCollision);

	//Spawn Elim Bot


	if (ElimBotEffect) {
		FVector ElimBotSpawnPoint(GetActorLocation().X, GetActorLocation().Y, GetActorLocation().Z + 200.f);
		ElimBotComponent=UGameplayStatics::SpawnEmitterAtLocation(GetWorld(), ElimBotEffect, ElimBotSpawnPoint, GetActorRotation());



	}
	
	if (ElimBotSound) {
		UGameplayStatics::PlaySoundAtLocation(this, ElimBotSound, GetActorLocation());


	}

	if (IsLocallyControlled() && Combat && Combat->bAiming && Combat->EquippedWeapon && Combat->EquippedWeapon->GetWeaponType() == E_WeaponType::EWT_SniperRifle) {
		ShowSniperScopeWidget(0);

	}


	GetWorldTimerManager().SetTimer(ElimTimer, this, &ABlasterCharacter::ElimTimerFinished, ElimDelay);







}

void ABlasterCharacter::Jump() {
	
	if (bIsCrouched) {
		UnCrouch();
	}
	else {
		Super::Jump();
	}



}

void ABlasterCharacter::OnRep_ReplicatedMovement() {

	Super::OnRep_ReplicatedMovement();
	SimProxiesTurn();


	TimeSinceLastMovRep = 0.f;

}

void ABlasterCharacter::Destroyed() {

	Super::Destroyed();

	if (ElimBotComponent) {
		ElimBotComponent->DestroyComponent();

	}

	ABlasterGameMode* BlasterGameMode = Cast<ABlasterGameMode>(UGameplayStatics::GetGameMode(this));
	bool bMatchNotInProgress = BlasterGameMode && BlasterGameMode->GetMatchState() != MatchState::InProgress;

	if (Combat && Combat->EquippedWeapon&& bMatchNotInProgress) {
		Combat->EquippedWeapon->Destroy();

	}



}


void ABlasterCharacter::TurnInPlace(float DeltaTime) {

	if (AO_Yaw > 90.f) {
		TurningInPlace = E_TurningInPlace::ETIP_Right;
	}
	else if (AO_Yaw < -90.f) {
		TurningInPlace = E_TurningInPlace::ETIP_Left;

	}
	if (TurningInPlace != E_TurningInPlace::ETIP_NotTurning) {
		InterpAO_Yaw = FMath::FInterpTo(InterpAO_Yaw, 0.f, DeltaTime, 4.f);
		AO_Yaw = InterpAO_Yaw;
		if (FMath::Abs(AO_Yaw) < 15.f) {
			TurningInPlace = E_TurningInPlace::ETIP_NotTurning;
			StartingAimRotation = FRotator(0.f, GetBaseAimRotation().Yaw, 0.f);

		}

	}



}

void ABlasterCharacter::HideCharacterIfCameraClose() {

	if (!IsLocallyControlled()) {
		return;
	}
	if ((FollowCamera->GetComponentLocation() - GetActorLocation()).Size() < CameraThreshold) {
		GetMesh()->SetVisibility(0);
		if (Combat && Combat->EquippedWeapon && Combat->EquippedWeapon->GetWeaponMesh()) {

			Combat->EquippedWeapon->GetWeaponMesh()->bOwnerNoSee = 1;

		}




	}
	else {
		GetMesh()->SetVisibility(1);
		if (Combat && Combat->EquippedWeapon && Combat->EquippedWeapon->GetWeaponMesh()) {

			Combat->EquippedWeapon->GetWeaponMesh()->bOwnerNoSee = 0;

		}
	}

}





