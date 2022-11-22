// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Pickups/Pickup.h"
#include "SheildPickup.generated.h"

/**
 * 
 */
UCLASS()
class BLASTERGAME_API ASheildPickup : public APickup
{
	GENERATED_BODY()

public:
	ASheildPickup();


protected:

	virtual void OnSphereOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult) override;


private:

	UPROPERTY(EditAnywhere, Category = HealthBuff)
		float HealAmount = 45.f;

	UPROPERTY(EditAnywhere, Category = HealthBuff)
		float HealingTime = 4.f;


};
