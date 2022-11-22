// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "TurningInPlace.h"
#include "Projectile.generated.h"

class UBoxComponent;
class UProjectileMovementComponent;
class UParticleSystem;
class UParticleSystemComponent;
class USoundCue;
class UNiagaraSystem;
class UNiagaraComponent;



UCLASS()
class BLASTERGAME_API AProjectile : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	AProjectile();


	// Called every frame
	virtual void Tick(float DeltaTime) override;

	virtual void Destroyed() override;


	//used With ServerSideRewind

	bool bUseServerSideRewind = 0;
	FVector_NetQuantize TraceStart;
	FVector_NetQuantize100 InitialVelocity;


	UPROPERTY(EditAnywhere)
		float InitialSpeed = 15000.f;

	
	float Damage = 5.f;






protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

	UFUNCTION()
	virtual void OnHit(UPrimitiveComponent* HitComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, FVector NormalImpulse, const FHitResult& Hit);

	

	UPROPERTY(EditAnywhere, Category = Components)
		UParticleSystem* ImpactParticles;

	UPROPERTY(EditAnywhere, Category = Components)
		USoundCue* ImpactSound;

	UPROPERTY(EditAnywhere, Category = Components)
		UBoxComponent* CollisionBox;


	UPROPERTY(VisibleAnywhere, Category = Components)
		UProjectileMovementComponent* ProjectileMovementComponent;

	UPROPERTY(EditAnywhere, Category = Components)
		UNiagaraSystem* TrailSystem;

	UPROPERTY(VisibleAnywhere, Category = Components)
		UNiagaraComponent* TrailSystemComponent;

	void SpawnTrailSystem();

	void StartDestroyTimer();

	void DestroyTimerFinished();

	UPROPERTY(VisibleAnywhere, Category = Components)
		UStaticMeshComponent* ProjectileMesh;

	void ExplodeDamage();


private:
	


	
	UPROPERTY(EditAnywhere, Category = Components)
		UParticleSystem* Tracer;

	UPROPERTY(VisibleAnywhere, Category = Components)
		UParticleSystemComponent* TracerComponent;

	
	FTimerHandle DestroyTimer;

	UPROPERTY(EditAnywhere, Category = Components)
		float DestroyTime = .8f;
	





public:	

	FORCEINLINE UBoxComponent* GetCollisionBox() const { return CollisionBox; }


	

};
