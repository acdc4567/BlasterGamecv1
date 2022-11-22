// Fill out your copyright notice in the Description page of Project Settings.


#include "Weapons/ProjectileRocket.h"
#include "Kismet/GameplayStatics.h"
#include "Components/BoxComponent.h"
#include "NiagaraFunctionLibrary.h"
#include "Sound/SoundCue.h"
#include "NiagaraComponent.h"
#include "Sound/SoundCue.h"
#include "Components/AudioComponent.h"
#include "Weapons/RocketMovementComponent.h"



AProjectileRocket::AProjectileRocket() {

	ProjectileMesh = CreateDefaultSubobject<UStaticMeshComponent>("RocketMesh");
	ProjectileMesh->SetupAttachment(GetRootComponent());
	ProjectileMesh->SetCollisionEnabled(ECollisionEnabled::NoCollision);

	if(GetCollisionBox())
	GetCollisionBox()->SetBoxExtent(FVector(14.f, 5.f, 5.f));

	RocketMovementComponent = CreateDefaultSubobject<URocketMovementComponent>("RocketMovementComponent");
	RocketMovementComponent->bRotationFollowsVelocity = 1;
	RocketMovementComponent->SetIsReplicated(1);
	RocketMovementComponent->InitialSpeed = 5000.f;
	RocketMovementComponent->MaxSpeed = 5000.f;



}

void AProjectileRocket::Destroyed() {




}

void AProjectileRocket::BeginPlay() {
	Super::BeginPlay();

	if (!HasAuthority()) {
		CollisionBox->OnComponentHit.AddDynamic(this, &AProjectileRocket::OnHit);

	}



	SpawnTrailSystem();

	if (ProjectileLoop && LoopingSoundAttenuation) {

		ProjectileLoopComponent = UGameplayStatics::SpawnSoundAttached(ProjectileLoop, GetRootComponent(), FName(), GetActorLocation(), EAttachLocation::KeepWorldPosition, 0, 1.f, 1.f, 0.f, LoopingSoundAttenuation, (USoundConcurrency*)nullptr, 0);

	}

}

void AProjectileRocket::OnHit(UPrimitiveComponent* HitComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, FVector NormalImpulse, const FHitResult& Hit) {
	if (OtherActor == GetOwner()) {
		return;

	}


	ExplodeDamage();

	StartDestroyTimer();

	if (ImpactParticles) {

		UGameplayStatics::SpawnEmitterAtLocation(GetWorld(), ImpactParticles, GetActorTransform());
	}
	if (ImpactSound) {
		UGameplayStatics::PlaySoundAtLocation(this, ImpactSound, GetActorLocation());
	}
	

	if (ProjectileMesh) {

		ProjectileMesh->SetVisibility(0);

	}
	if (CollisionBox) {
		CollisionBox->SetCollisionEnabled(ECollisionEnabled::NoCollision);


	}
	if (TrailSystemComponent&& TrailSystemComponent->GetSystemInstanceController()->GetSoloSystemInstance()) {
		TrailSystemComponent->GetSystemInstanceController()->GetSoloSystemInstance()->Deactivate();
	}

	if (ProjectileLoopComponent && ProjectileLoopComponent->IsPlaying()) {
		ProjectileLoopComponent->Stop();
	}




}


