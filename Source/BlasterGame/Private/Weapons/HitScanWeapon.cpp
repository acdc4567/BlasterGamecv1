// Fill out your copyright notice in the Description page of Project Settings.


#include "Weapons/HitScanWeapon.h"
#include "Engine/SkeletalMeshSocket.h"
#include "Character/BlasterCharacter.h"
#include "Kismet/GameplayStatics.h"
#include "Particles/ParticleSystemComponent.h"
#include "Sound/SoundCue.h"
#include "DrawDebugHelpers.h"
#include "BlasterComponents/LagCompensationComponent.h"
#include "PlayerControllers/BlasterPlayerController.h"




void AHitScanWeapon::Fire(const FVector& HitTarget) {
	Super::Fire(HitTarget);

	APawn* OwnerPawn = Cast<APawn>(GetOwner());
	if (OwnerPawn == nullptr) {
		return;

	}
	AController* InstigatorController = OwnerPawn->GetController();



	const USkeletalMeshSocket* MuzzleFlashSocket = GetWeaponMesh()->GetSocketByName("MuzzleFlash");
	if (MuzzleFlashSocket) {
		FTransform SocketTransform = MuzzleFlashSocket->GetSocketTransform(GetWeaponMesh());
		FVector Start = SocketTransform.GetLocation();
		

		FHitResult FireHit;
		WeaponTraceHit(Start, HitTarget, FireHit);

		ABlasterCharacter* BlasterCharacterx = Cast<ABlasterCharacter>(FireHit.GetActor());
		if (BlasterCharacterx && InstigatorController) {
			bool bCauseAuthDamage = !bUseServerSideRewind || OwnerPawn->IsLocallyControlled();
			if (HasAuthority() && bCauseAuthDamage) {
				UGameplayStatics::ApplyDamage(BlasterCharacterx, Damage, InstigatorController, this, UDamageType::StaticClass());

			}
			if (!HasAuthority() && bUseServerSideRewind) {
				BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(OwnerPawn) : BlasterCharacter;
				BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(InstigatorController) : BlasterPlayerController;

				if (BlasterCharacter && BlasterPlayerController && BlasterCharacter->GetLagCompensation() && BlasterCharacter->IsLocallyControlled()) {
					BlasterCharacter->GetLagCompensation()->Server_ScoreRequest(BlasterCharacterx, Start, HitTarget, BlasterPlayerController->GetServerTime() - BlasterPlayerController->SingleTripTime, this);
				}



			}
			


		}

		if (ImpactParticles) {
			UGameplayStatics::SpawnEmitterAtLocation(GetWorld(), ImpactParticles, FireHit.ImpactPoint, FireHit.ImpactNormal.Rotation());


		}

		if (HitSound) {
			UGameplayStatics::PlaySoundAtLocation(this, HitSound, FireHit.ImpactPoint);

		}



		
		if (MuzzleFlash) {
			UGameplayStatics::SpawnEmitterAtLocation(GetWorld(), MuzzleFlash, SocketTransform);

		}
		if (FireSound) {
			UGameplayStatics::PlaySoundAtLocation(this, FireSound, GetActorLocation());

		}

	}







}



void AHitScanWeapon::WeaponTraceHit(const FVector& TraceStart, const FVector& HitTarget, FHitResult& OutHit) {

	
	UWorld* World= GetWorld();
	if (World) {

		FVector End =  TraceStart + (HitTarget - TraceStart) * 1.25f;

		FCollisionQueryParams QueryParams;
		QueryParams.AddIgnoredActor(GetOwner());
		World->LineTraceSingleByChannel(OutHit, TraceStart, End, ECollisionChannel::ECC_Visibility, QueryParams);
		FVector BeamEnd = End;
		if (OutHit.bBlockingHit) {
			BeamEnd = OutHit.ImpactPoint;

		}

		//DrawDebugSphere(GetWorld(), BeamEnd, 16.f, 12, FColor::Cyan, 1);



		if (BeamParticles) {
			UParticleSystemComponent* Beam = UGameplayStatics::SpawnEmitterAtLocation(World, BeamParticles, TraceStart, FRotator::ZeroRotator, 1);
			if (Beam) {
				Beam->SetVectorParameter("Target", BeamEnd);
			}

		}






	}


}
