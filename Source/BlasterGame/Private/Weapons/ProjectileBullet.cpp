// Fill out your copyright notice in the Description page of Project Settings.


#include "Weapons/ProjectileBullet.h"
#include "Kismet/GameplayStatics.h"
#include "Character/BlasterCharacter.h"
#include "PlayerControllers/BlasterPlayerController.h"
#include "BlasterComponents/LagCompensationComponent.h"

#include "GameFramework/ProjectileMovementComponent.h"



AProjectileBullet::AProjectileBullet() {

	ProjectileMovementComponent = CreateDefaultSubobject<UProjectileMovementComponent>("ProjectileMovementComponent");
	ProjectileMovementComponent->bRotationFollowsVelocity = 1;
	ProjectileMovementComponent->SetIsReplicated(1);
	ProjectileMovementComponent->InitialSpeed = InitialSpeed;
	ProjectileMovementComponent->MaxSpeed = InitialSpeed;



}


#if WITH_EDITOR
void AProjectileBullet::PostEditChangeProperty(FPropertyChangedEvent& PropertyChangedEvent) {
	Super::PostEditChangeProperty(PropertyChangedEvent);

	FName PropertyName = PropertyChangedEvent.Property != nullptr ? PropertyChangedEvent.Property->GetFName() : NAME_None;
	if (PropertyName == GET_MEMBER_NAME_CHECKED(AProjectileBullet, InitialSpeed)) {
		if (ProjectileMovementComponent) {
			ProjectileMovementComponent->InitialSpeed = InitialSpeed;
			ProjectileMovementComponent->MaxSpeed = InitialSpeed;



		}

	}




}
#endif




void AProjectileBullet::OnHit(UPrimitiveComponent* HitComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, FVector NormalImpulse, const FHitResult& Hit) {

	ABlasterCharacter* OwnerCharacter=Cast<ABlasterCharacter>(GetOwner());
	if (OwnerCharacter) {
		ABlasterPlayerController* OwnerController = Cast<ABlasterPlayerController>(OwnerCharacter->Controller);
		if (OwnerController) {

			if (OwnerCharacter->HasAuthority() && !bUseServerSideRewind) {

				UGameplayStatics::ApplyDamage(OtherActor, Damage, OwnerController, this, UDamageType::StaticClass());;
				Super::OnHit(HitComponent, OtherActor, OtherComp, NormalImpulse, Hit);
				return;

			}

			ABlasterCharacter* HitCharacter = Cast<ABlasterCharacter>(OtherActor);
			if (bUseServerSideRewind && OwnerCharacter->GetLagCompensation() && OwnerCharacter->IsLocallyControlled() && HitCharacter) {
				OwnerCharacter->GetLagCompensation()->Server_ProjectileScoreRequest(HitCharacter, TraceStart, InitialVelocity, OwnerController->GetServerTime() - OwnerController->SingleTripTime);

			}

		}
	}




	Super::OnHit(HitComponent, OtherActor, OtherComp, NormalImpulse, Hit);


}

void AProjectileBullet::BeginPlay() {

	Super::BeginPlay();

	/*
	FPredictProjectilePathParams PathParams;
	PathParams.bTraceWithChannel = 1;
	PathParams.bTraceWithCollision = 1;
	PathParams.DrawDebugTime = 5.f;
	PathParams.DrawDebugType = EDrawDebugTrace::ForDuration;
	PathParams.LaunchVelocity = GetActorForwardVector() * InitialSpeed;
	PathParams.MaxSimTime = 4.f;
	PathParams.ProjectileRadius = 5.f;
	PathParams.SimFrequency = 30.f;
	PathParams.StartLocation = GetActorLocation();
	PathParams.TraceChannel = ECollisionChannel::ECC_Visibility;
	PathParams.ActorsToIgnore.Add(this);




	FPredictProjectilePathResult PathResult;

	UGameplayStatics::PredictProjectilePath(this, PathParams, PathResult);
	
	
	*/








}
