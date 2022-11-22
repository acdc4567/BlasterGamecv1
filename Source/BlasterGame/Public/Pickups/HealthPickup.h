// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Pickups/Pickup.h"
#include "HealthPickup.generated.h"






/**
 * 
 */
UCLASS()
class BLASTERGAME_API AHealthPickup : public APickup
{
	GENERATED_BODY()
	

public:
	AHealthPickup();

	




protected:

	virtual void OnSphereOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult) override;


private:

	UPROPERTY(EditAnywhere, Category = HealthBuff)
		float HealAmount = 45.f;

	UPROPERTY(EditAnywhere, Category = HealthBuff)
		float HealingTime = 4.f;

	








};
