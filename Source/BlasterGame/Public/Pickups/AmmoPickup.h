// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Pickups/Pickup.h"
#include "TurningInPlace.h"
#include "AmmoPickup.generated.h"

/**
 * 
 */
UCLASS()
class BLASTERGAME_API AAmmoPickup : public APickup
{
	GENERATED_BODY()
	
public:






protected:

	virtual void OnSphereOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult) override;







private:

	UPROPERTY(EditAnywhere, Category = Pickups)
		int32 AmmoAmount = 30;

	UPROPERTY(EditAnywhere, Category = Pickups)
		E_WeaponType WeaponType = E_WeaponType::EWT_AssaultRifle;



public:









};
