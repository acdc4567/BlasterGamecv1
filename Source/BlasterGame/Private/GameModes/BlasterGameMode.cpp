// Fill out your copyright notice in the Description page of Project Settings.


#include "GameModes/BlasterGameMode.h"
#include "Character/BlasterCharacter.h"
#include "PlayerControllers/BlasterPlayerController.h"
#include "Kismet/GameplayStatics.h"
#include "GameFramework/PlayerStart.h"
#include "PlayerStates/BlasterPlayerState.h"
#include "GameStates/BlasterGameState.h"




namespace MatchState {
	const FName Cooldown = FName("Cooldown");
}


ABlasterGameMode::ABlasterGameMode() {

	bDelayedStart = 1;





}

void ABlasterGameMode::Tick(float DeltaTime) {
	Super::Tick(DeltaTime);

	if (MatchState == MatchState::WaitingToStart) {
		CountdownTime = WarmUpTime - GetWorld()->GetTimeSeconds() + LevelStartingTime;

		if (CountdownTime <= 0.f) {
			StartMatch();

		}

	}
	else if (MatchState == MatchState::InProgress) {

		CountdownTime = WarmUpTime + MatchTime - GetWorld()->GetTimeSeconds() + LevelStartingTime;
		if (CountdownTime <= 0.f) {
			SetMatchState(MatchState::Cooldown);
		}

	}
	else if (MatchState == MatchState::Cooldown)
	{
		CountdownTime = CooldownTime + WarmUpTime + MatchTime - GetWorld()->GetTimeSeconds() + LevelStartingTime;
		if (CountdownTime <= 0.f) {
			RestartGame();
		}
		
	}


}

void ABlasterGameMode::PlayerEliminated(ABlasterCharacter* ElimmedCharacter, ABlasterPlayerController* VictimController, ABlasterPlayerController* AttackerController) {
	
	if (AttackerController == nullptr || AttackerController->PlayerState == nullptr)return;
	if (VictimController == nullptr || VictimController->PlayerState == nullptr)return;


	ABlasterPlayerState* AttackerPlayerState = AttackerController ? Cast<ABlasterPlayerState>(AttackerController->PlayerState) : nullptr;
	
	ABlasterPlayerState* VictimPlayerState = VictimController ? Cast<ABlasterPlayerState>(VictimController->PlayerState) : nullptr;

	ABlasterGameState* BlasterGameState = GetGameState<ABlasterGameState>();


	if (AttackerPlayerState && AttackerPlayerState != VictimPlayerState&& BlasterGameState) {
		AttackerPlayerState->AddToScore(10.f);

		BlasterGameState->UpdateTopScore(AttackerPlayerState);


	}
	if (VictimPlayerState) {
		VictimPlayerState->AddToDefeats(1);
	}


	if (ElimmedCharacter) {
		ElimmedCharacter->Elim(0);
	}




}

void ABlasterGameMode::RequestRespawn(ACharacter* ElimmedCharacter, AController* ElimmedController) {

	if (ElimmedCharacter) {
		ElimmedCharacter->Reset();
		ElimmedCharacter->Destroy();
	}
	if (ElimmedController) {
		TArray<AActor* > PlayerStarts;
		UGameplayStatics::GetAllActorsOfClass(this,APlayerStart::StaticClass(), PlayerStarts);
		int32 SelectionIndex = FMath::RandRange(0, PlayerStarts.Num() - 1);

		RestartPlayerAtPlayerStart(ElimmedController, PlayerStarts[SelectionIndex]);
	}




}



void ABlasterGameMode::PlayerLeftGame(ABlasterPlayerState* PlayerLeaving) {
	if (PlayerLeaving == nullptr)return;

	ABlasterGameState* BlasterGameState = GetGameState<ABlasterGameState>();
	if (BlasterGameState && BlasterGameState->TopScoringPlayers.Contains(PlayerLeaving)) {
		BlasterGameState->TopScoringPlayers.Remove(PlayerLeaving);

	}
	ABlasterCharacter* CharacterLeaving = Cast<ABlasterCharacter>(PlayerLeaving->GetPawn());

	if (CharacterLeaving) {
		CharacterLeaving->Elim(1);


	}


}



void ABlasterGameMode::BeginPlay() {
	Super::BeginPlay();

	LevelStartingTime = GetWorld()->GetTimeSeconds();



}

void ABlasterGameMode::OnMatchStateSet() {

	Super::OnMatchStateSet();


	for (FConstPlayerControllerIterator It = GetWorld()->GetPlayerControllerIterator(); It; ++It) {
		ABlasterPlayerController* BlasterPlayerController = Cast<ABlasterPlayerController>(*It);
		if (BlasterPlayerController) {
			BlasterPlayerController->OnMatchStateSet(MatchState);


		}


	}



}
