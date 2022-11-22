// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameMode.h"
#include "BlasterGameMode.generated.h"


class ABlasterCharacter;
class ABlasterPlayerController;
class ABlasterPlayerState;



namespace MatchState {
	extern BLASTERGAME_API const FName Cooldown;
}


/**
 * 
 */
UCLASS()
class BLASTERGAME_API ABlasterGameMode : public AGameMode
{
	GENERATED_BODY()
	
public:
	ABlasterGameMode();

	virtual void Tick(float DeltaTime) override;

	virtual void PlayerEliminated(ABlasterCharacter* ElimmedCharacter,ABlasterPlayerController* VictimController, ABlasterPlayerController* AttackerController);

	virtual void RequestRespawn(ACharacter* ElimmedCharacter, AController* ElimmedController);

	void PlayerLeftGame(ABlasterPlayerState* PlayerLeaving);

	UPROPERTY(EditDefaultsOnly)
		float WarmUpTime = 10.f;

	UPROPERTY(EditDefaultsOnly)
		float MatchTime = 200.f;

	UPROPERTY(EditDefaultsOnly)
		float CooldownTime = 10.f;

	float LevelStartingTime = 0.f;

protected:

	virtual void BeginPlay() override;

	virtual void OnMatchStateSet() override;







private:

	float CountdownTime = 0.f;


public:

	FORCEINLINE float GetCountdownTime() const { return CountdownTime; }




};
