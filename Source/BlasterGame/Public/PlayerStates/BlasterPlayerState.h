// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerState.h"
#include "BlasterPlayerState.generated.h"


class ABlasterCharacter;
class ABlasterPlayerController;







/**
 * 
 */
UCLASS()
class BLASTERGAME_API ABlasterPlayerState : public APlayerState
{
	GENERATED_BODY()
public:

	virtual void OnRep_Score() override;

	void AddToScore(float ScoreAmount);

	void AddToDefeats(int32 ScoreAmount);



	void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

	UFUNCTION()
		virtual void OnRep_Defeats();

private:

	UPROPERTY(VisibleAnywhere, Category = Components)
		ABlasterCharacter* BlasterCharacter;

	UPROPERTY(VisibleAnywhere, Category = Components)
		ABlasterPlayerController* BlasterPlayerController;

	UPROPERTY(ReplicatedUsing=OnRep_Defeats, VisibleAnywhere, Category = Components)
		int32 Defeats = 0;
	
	






};
