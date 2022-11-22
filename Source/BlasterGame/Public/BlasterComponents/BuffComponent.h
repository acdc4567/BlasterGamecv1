// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "BuffComponent.generated.h"

class ABlasterCharacter;








UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class BLASTERGAME_API UBuffComponent : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UBuffComponent();
	friend class ABlasterCharacter;

	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	void Heal(float HealAmount, float HealingTime);

	void ReplenishSheild(float HealAmount, float HealingTime);


	void BuffSpeed(float BuffBaseSpeed, float BuffCrouchSpeed, float BuffTime);

	void BuffJump(float BuffJumpVelocity, float BuffTime);

	void SetInitialSpeeds(float BaseSpeed, float CrouchSpeed);
	void SetInitialJumpVelocity(float JumpVelocity);







protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	void HealRampUp(float DeltaTime);

	void SheildRampUp(float DeltaTime);





private:

	UPROPERTY(VisibleAnywhere, Category = Characters)
		ABlasterCharacter* Character;



	//HealBuff

	bool bHealing = 0;

	float HealingRate = 0.f;

	float AmountToHeal = 0.f;

	//SheildBuff


	bool bSheilding = 0;

	float SheildingRate = 0.f;

	float AmountToSheild = 0.f;



	//SpeedBuff

	FTimerHandle SpeedBuffTimer;
	void ResetSpeeds();


	float InitialBaseSpeed = 0.f;
	float InitialCrouchSpeed = 0.f;


	UFUNCTION(NetMulticast, Reliable)
		void Multicast_SpeedBuff(float BaseSpeed,float CrouchSpeed);


	//JumpBuff
	
	FTimerHandle JumpBuffTimer;
	void ResetJump();
	float InitialJumpVelocity = 0.f;

	UFUNCTION(NetMulticast, Reliable)
		void Multicast_JumpBuff(float JumpSpeed);





public:	
	
		
};
