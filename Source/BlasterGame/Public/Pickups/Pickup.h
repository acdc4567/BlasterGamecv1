// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "TurningInPlace.h"

#include "Pickup.generated.h"

class USphereComponent;
class USoundCue;
class UNiagaraComponent;
class UNiagaraSystem;






UCLASS()
class BLASTERGAME_API APickup : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	APickup();

	// Called every frame
	virtual void Tick(float DeltaTime) override;

	virtual void Destroyed() override;






protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;


	UFUNCTION()
		virtual void OnSphereOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);

	UPROPERTY(EditAnywhere, Category = Components)
		float BaseTurnRate = 45.f;





private:
	UPROPERTY(EditAnywhere, Category = Components)
		USphereComponent* OverlapSphere;

	UPROPERTY(EditAnywhere, Category = Components)
		USoundCue* PickUpSound;

	UPROPERTY(VisibleAnywhere, Category = Components)
		UStaticMeshComponent* PickupMesh;

	UPROPERTY(VisibleAnywhere, Category = HealthBuff)
		UNiagaraComponent* PickupEffectComponent;

	UPROPERTY(EditAnywhere, Category = HealthBuff)
		UNiagaraSystem* PickupEffect;


	FTimerHandle BindOverlapTimer;

	float BindOverlapTime = .25f;

	void BindOverlapTimerFinished();





public:	










	

};
