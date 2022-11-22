// Fill out your copyright notice in the Description page of Project Settings.


#include "Weapons/Shotgun.h"
#include "Engine/SkeletalMeshSocket.h"
#include "Kismet/GameplayStatics.h"
#include "Particles/ParticleSystemComponent.h"
#include "Sound/SoundCue.h"
#include "Character/BlasterCharacter.h"
#include "Kismet/KismetMathLibrary.h"
#include "PlayerControllers/BlasterPlayerController.h"
#include "BlasterComponents/LagCompensationComponent.h"






void AShotgun::FireShotgun(const TArray<FVector_NetQuantize>& HitTargets) {

	AWeapon::Fire(FVector());

	APawn* OwnerPawn = Cast<APawn>(GetOwner());
	if (OwnerPawn == nullptr) {
		return;

	}
	AController* InstigatorController = OwnerPawn->GetController();

	const USkeletalMeshSocket* MuzzleFlashSocket = GetWeaponMesh()->GetSocketByName("MuzzleFlash");
	if (MuzzleFlashSocket) {
		const FTransform SocketTransform = MuzzleFlashSocket->GetSocketTransform(GetWeaponMesh());

		const FVector Start = SocketTransform.GetLocation();

		TMap<ABlasterCharacter*, uint32> HitMap;

		for (auto HitTarget : HitTargets) {
			FHitResult FireHit;
			WeaponTraceHit(Start, HitTarget, FireHit);

			ABlasterCharacter* BlasterCharacterx = Cast<ABlasterCharacter>(FireHit.GetActor());
			if (BlasterCharacterx ) {

				
				if (HitMap.Contains(BlasterCharacterx)) {
					HitMap[BlasterCharacterx]++;


				}
				else {
					HitMap.Emplace(BlasterCharacterx, 1);
				}


				if (ImpactParticles) {
					UGameplayStatics::SpawnEmitterAtLocation(GetWorld(), ImpactParticles, FireHit.ImpactPoint, FireHit.ImpactNormal.Rotation());

				}

				if (HitSound) {
					UGameplayStatics::PlaySoundAtLocation(this, HitSound, FireHit.ImpactPoint, .5f, FMath::FRandRange(-.5f, .5f));

				}

			}


		}

		TArray<ABlasterCharacter* > HitCharacters;


		for (auto HitPair : HitMap) {
			if (HitPair.Key  && InstigatorController) {
				bool bCauseAuthDamage = !bUseServerSideRewind || OwnerPawn->IsLocallyControlled();
				if (HasAuthority() && bCauseAuthDamage) {
					UGameplayStatics::ApplyDamage(HitPair.Key, Damage * HitPair.Value, InstigatorController, this, UDamageType::StaticClass());

				}

				HitCharacters.Add(HitPair.Key);

			}

		}


		if (!HasAuthority() && bUseServerSideRewind) {
			BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(OwnerPawn) : BlasterCharacter;
			BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(InstigatorController) : BlasterPlayerController;

			if (BlasterCharacter && BlasterPlayerController && BlasterCharacter->GetLagCompensation() && BlasterCharacter->IsLocallyControlled()) {
				BlasterCharacter->GetLagCompensation()->Server_ShotgunScoreRequest(HitCharacters, Start, HitTargets, BlasterPlayerController->GetServerTime() - BlasterPlayerController->SingleTripTime);
			}


		}



	}












}

void AShotgun::ShotgunTraceEndWithScatter(const FVector& HitTarget, TArray<FVector_NetQuantize>& HitTargets) {

	const USkeletalMeshSocket* MuzzleFlashSocket = GetWeaponMesh()->GetSocketByName("MuzzleFlash");
	if (MuzzleFlashSocket == nullptr)return;

	const FTransform SocketTransform = MuzzleFlashSocket->GetSocketTransform(GetWeaponMesh());
	const FVector TraceStart = SocketTransform.GetLocation();

	const FVector ToTargetNormalized = (HitTarget - TraceStart).GetSafeNormal();
	const FVector SphereCenter = TraceStart + ToTargetNormalized * DistanceToSphere;







	for (uint32 i = 0; i < NumberOfShotgunPellets; i++) {
		const FVector RandVec = UKismetMathLibrary::RandomUnitVector() * FMath::FRandRange(0.f, SphereRadius);
		const FVector EndLoc = SphereCenter + RandVec;
		FVector ToEndLoc = EndLoc - TraceStart;

		ToEndLoc = TraceStart + ToEndLoc * TRACE_LENGTH / ToEndLoc.Size();
		HitTargets.Add(ToEndLoc);



	}




}
