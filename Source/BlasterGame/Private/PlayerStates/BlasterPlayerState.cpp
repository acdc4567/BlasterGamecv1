// Fill out your copyright notice in the Description page of Project Settings.


#include "PlayerStates/BlasterPlayerState.h"
#include "Character/BlasterCharacter.h"
#include "PlayerControllers/BlasterPlayerController.h"
#include "Net/UnrealNetwork.h"



void ABlasterPlayerState::AddToScore(float ScoreAmount) {

	SetScore(GetScore() + ScoreAmount);

	BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(GetPawn()) : BlasterCharacter;

	if (BlasterCharacter) {
		BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(BlasterCharacter->Controller) : BlasterPlayerController;
		if (BlasterPlayerController) {
			BlasterPlayerController->SetHUDScore(GetScore());



		}

	}

}

void ABlasterPlayerState::AddToDefeats(int32 ScoreAmount) {

	Defeats += ScoreAmount;
	BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(GetPawn()) : BlasterCharacter;

	if (BlasterCharacter) {
		BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(BlasterCharacter->Controller) : BlasterPlayerController;
		if (BlasterPlayerController) {
			BlasterPlayerController->SetHUDDefeats(Defeats);



		}

	}


}

void ABlasterPlayerState::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const {
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(ABlasterPlayerState,Defeats);




}

void ABlasterPlayerState::OnRep_Defeats() {

	BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(GetPawn()) : BlasterCharacter;

	if (BlasterCharacter) {
		BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(BlasterCharacter->Controller) : BlasterPlayerController;
		if (BlasterPlayerController) {

			BlasterPlayerController->SetHUDDefeats(Defeats);



		}

	}



}

void ABlasterPlayerState::OnRep_Score() {

	Super::OnRep_Score();

	BlasterCharacter = BlasterCharacter == nullptr ? Cast<ABlasterCharacter>(GetPawn()) : BlasterCharacter;

	if (BlasterCharacter) {
		BlasterPlayerController = BlasterPlayerController == nullptr ? Cast<ABlasterPlayerController>(BlasterCharacter->Controller) : BlasterPlayerController;
		if (BlasterPlayerController) {

			BlasterPlayerController->SetHUDScore(GetScore());



		}

	}





}


