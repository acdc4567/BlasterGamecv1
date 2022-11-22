// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Weapons/Projectile.h"
#include "ProjectileRocket.generated.h"

class UNiagaraSystem;
class UNiagaraComponent;
class USoundCue;
class URocketMovementComponent;



/**
 * 
 */
UCLASS()
class BLASTERGAME_API AProjectileRocket : public AProjectile
{
	GENERATED_BODY()
public:
	AProjectileRocket();

	virtual void Destroyed() override;





protected:
	virtual void BeginPlay() override;

	virtual void OnHit(UPrimitiveComponent* HitComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, FVector NormalImpulse, const FHitResult& Hit) override;

	

	
	UPROPERTY(EditAnywhere, Category = Components)
		USoundCue* ProjectileLoop;

	UPROPERTY(VisibleAnywhere, Category = Components)
		UAudioComponent* ProjectileLoopComponent;

	UPROPERTY(EditAnywhere, Category = Components)
		USoundAttenuation* LoopingSoundAttenuation;

	UPROPERTY(VisibleAnywhere, Category = Components)
		URocketMovementComponent* RocketMovementComponent;





private:

	
	














};
