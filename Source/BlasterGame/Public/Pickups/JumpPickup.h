// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Pickups/Pickup.h"
#include "JumpPickup.generated.h"

/**
 * 
 */
UCLASS()
class BLASTERGAME_API AJumpPickup : public APickup
{
	GENERATED_BODY()
	

protected:

	virtual void OnSphereOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult) override;

private:

	UPROPERTY(EditAnywhere, Category = Components)
		float JumpZVelocityBuff = 4000.f;

	UPROPERTY(EditAnywhere, Category = Components)
		float JumpZBuffTime = 10.f;






};
