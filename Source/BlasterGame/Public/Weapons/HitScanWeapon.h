// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Weapons/Weapon.h"
#include "TurningInPlace.h"

#include "HitScanWeapon.generated.h"

class UParticleSystem;
class USoundCue;




/**
 * 
 */
UCLASS()
class BLASTERGAME_API AHitScanWeapon : public AWeapon
{
	GENERATED_BODY()
	
public:

	virtual void Fire(const FVector& HitTarget) override;

	



protected:

	

	void WeaponTraceHit(const FVector& TraceStart, const FVector& HitTarget, FHitResult& OutHit);

	UPROPERTY(EditAnywhere, Category = Camera)
		UParticleSystem* ImpactParticles;

	UPROPERTY(EditAnywhere, Category = Camera)
		USoundCue* HitSound;

	

private:

	

	

	UPROPERTY(EditAnywhere, Category = Camera)
		UParticleSystem* BeamParticles;

	UPROPERTY(EditAnywhere, Category = Camera)
		UParticleSystem* MuzzleFlash;

	UPROPERTY(EditAnywhere, Category = Camera)
		USoundCue* FireSound;

	
	
	






};
