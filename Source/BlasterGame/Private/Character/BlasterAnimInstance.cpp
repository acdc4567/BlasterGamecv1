// Fill out your copyright notice in the Description page of Project Settings.


#include "Character/BlasterAnimInstance.h"
#include "Character/BlasterCharacter.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Kismet/KismetMathLibrary.h"
#include "Weapons/Weapon.h"








void UBlasterAnimInstance::NativeInitializeAnimation() {
	Super::NativeInitializeAnimation();


	BlasterCharacter = Cast<ABlasterCharacter>(TryGetPawnOwner());

}

void UBlasterAnimInstance::NativeUpdateAnimation(float DeltaSecond) {
	Super::NativeUpdateAnimation( DeltaSecond);

	if (BlasterCharacter == nullptr) {
		BlasterCharacter = Cast<ABlasterCharacter>(TryGetPawnOwner());


	}
	
	if (BlasterCharacter == nullptr) {
		return;

	}

	FVector Velocity = BlasterCharacter->GetVelocity();
	Velocity.Z = 0.f;
	Speed = Velocity.Size();

	bIsInAir = BlasterCharacter->GetCharacterMovement()->IsFalling();

	bIsAccelerating = BlasterCharacter->GetCharacterMovement()->GetCurrentAcceleration().Size() > 0.f ? 1 : 0;

	bWeaponEquipped = BlasterCharacter->IsWeaponEquipped();

	EquippedWeapon = BlasterCharacter->GetEquippedWeapon();

	bIsCrouched = BlasterCharacter->bIsCrouched;

	bAiming = BlasterCharacter->GetIsAiming();

	TurningInPlace = BlasterCharacter->GetTurningInPlace();

	bRotateRootBone = BlasterCharacter->GetShouldRotateRootBone();

	bElimmed = BlasterCharacter->GetElimmed();

	FRotator AimRotation = BlasterCharacter->GetBaseAimRotation();
	FRotator MovementRotation = UKismetMathLibrary::MakeRotFromX(BlasterCharacter->GetVelocity());
	FRotator DeltaRot= UKismetMathLibrary::NormalizedDeltaRotator(MovementRotation, AimRotation);
	DeltaRotation = FMath::RInterpTo(DeltaRotation, DeltaRot, DeltaSecond, 15.f);

	YawOffset = DeltaRotation.Yaw;

	CharacterRotationLastFrame = CharacterRotation;
	CharacterRotation = BlasterCharacter->GetActorRotation();

	const FRotator Delta = UKismetMathLibrary::NormalizedDeltaRotator(CharacterRotation, CharacterRotationLastFrame);
	const float Target = Delta.Yaw / DeltaSecond;
	const float Interp = FMath::FInterpTo(Lean, Target,DeltaSecond, 6.f);
	Lean = FMath::Clamp(Interp, -90.f, 90.f);

	AO_Yaw = BlasterCharacter->GetAO_Yaw();

	AO_Pitch = BlasterCharacter->GetAO_Pitch();

	if (bWeaponEquipped && EquippedWeapon && EquippedWeapon->GetWeaponMesh() && BlasterCharacter->GetMesh()) {
		LeftHandTransform = EquippedWeapon->GetWeaponMesh()->GetSocketTransform("LeftHandSocket", ERelativeTransformSpace::RTS_World);
		FRotator OutRotation;
		FVector OutLocation;
		BlasterCharacter->GetMesh()->TransformToBoneSpace("hand_r", LeftHandTransform.GetLocation(), FRotator::ZeroRotator, OutLocation, OutRotation);
		LeftHandTransform.SetLocation(OutLocation);
		LeftHandTransform.SetRotation(FQuat(OutRotation));


		if (BlasterCharacter->IsLocallyControlled()) {
			bLocallyControlled = 1;
			FTransform RightHandTransform = EquippedWeapon->GetWeaponMesh()->GetSocketTransform("hand_R", ERelativeTransformSpace::RTS_World);
			FRotator LookAtRotation= UKismetMathLibrary::FindLookAtRotation(RightHandTransform.GetLocation(), RightHandTransform.GetLocation() + (RightHandTransform.GetLocation() - BlasterCharacter->GetHitTarget()));
			RightHandRotation = FMath::RInterpTo(RightHandRotation, LookAtRotation, DeltaSecond, 30.f);

		}
		
		bUseFABRIK = BlasterCharacter->GetCombatState() == E_CombatState::ECS_UnOccupied;

		if (BlasterCharacter->IsLocallyControlled() && BlasterCharacter->GetCombatState() != E_CombatState::ECS_ThrowingGrenade && BlasterCharacter->bFinishedSwapping) {
			bUseFABRIK = !BlasterCharacter->GetIsLocallyReloading() ;

		}

		bUseAimOffsets = BlasterCharacter->GetCombatState() == E_CombatState::ECS_UnOccupied && BlasterCharacter->GetDisableGameplay();
		
		bTransformRightHand = BlasterCharacter->GetCombatState() == E_CombatState::ECS_UnOccupied && BlasterCharacter->GetDisableGameplay();




	}




}
