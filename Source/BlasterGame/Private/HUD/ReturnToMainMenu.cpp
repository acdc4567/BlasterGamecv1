// Fill out your copyright notice in the Description page of Project Settings.


#include "HUD/ReturnToMainMenu.h"
#include "GameFramework/PlayerController.h"
#include "Components/Button.h"
#include "MultiplayerSessionsSubsystem.h"
#include "GameFramework/GameModeBase.h"
#include "Character/BlasterCharacter.h"







void UReturnToMainMenu::MenuSetup() {
	AddToViewport();
	SetVisibility(ESlateVisibility::Visible);
	bIsFocusable = 1;
	UWorld* World = GetWorld();

	if (World) {
		PlayerController = PlayerController == nullptr ? World->GetFirstPlayerController() : PlayerController;
		if (PlayerController) {
			FInputModeGameAndUI InputModeData;
			InputModeData.SetWidgetToFocus(TakeWidget());
			PlayerController->SetInputMode(InputModeData);
			PlayerController->SetShowMouseCursor(1);

		}


	}

	if (ReturnButton && !ReturnButton->OnClicked.IsBound()) {
		ReturnButton->OnClicked.AddDynamic(this, &UReturnToMainMenu::ReturnButtonClicked);

	}

	UGameInstance* GameInstance = GetGameInstance();
	if (GameInstance) {
		MultiplayerSessionsSubsystem = GameInstance->GetSubsystem<UMultiplayerSessionsSubsystem>();
		if (MultiplayerSessionsSubsystem) {
			MultiplayerSessionsSubsystem->MultiplayerOnDestroySessionComplete.AddDynamic(this, &UReturnToMainMenu::OnDestroySession);


		}

	}





}

void UReturnToMainMenu::MenuTeardown() {

	RemoveFromParent();
	UWorld* World = GetWorld();

	if (World) {
		PlayerController = PlayerController == nullptr ? World->GetFirstPlayerController() : PlayerController;
		if (PlayerController) {
			FInputModeGameOnly InputModeData;
			
			PlayerController->SetInputMode(InputModeData);
			PlayerController->SetShowMouseCursor(0);

		}


	}

	if (ReturnButton && ReturnButton->OnClicked.IsBound()) {
		ReturnButton->OnClicked.RemoveDynamic(this, &UReturnToMainMenu::ReturnButtonClicked);

	}

	if (MultiplayerSessionsSubsystem&& MultiplayerSessionsSubsystem->MultiplayerOnDestroySessionComplete.IsBound()) {
		MultiplayerSessionsSubsystem->MultiplayerOnDestroySessionComplete.RemoveDynamic(this, &UReturnToMainMenu::OnDestroySession);


	}


}

bool UReturnToMainMenu::Initialize() {

	if (!Super::Initialize())return 0;
	
	


	return 1;
}

void UReturnToMainMenu::OnDestroySession(bool bWasSuccessful) {

	if (!bWasSuccessful) {
		ReturnButton->SetIsEnabled(1);
		return;
	}


	UWorld* World = GetWorld();

	if (World) {
		AGameModeBase* GameMode = World->GetAuthGameMode<AGameModeBase>();
		if (GameMode) {
			GameMode->ReturnToMainMenuHost();

		}
		else {
			PlayerController = PlayerController == nullptr ? World->GetFirstPlayerController() : PlayerController;
			if (PlayerController) {
				PlayerController->ClientReturnToMainMenuWithTextReason(FText());

			}


		}

	}


}

void UReturnToMainMenu::OnPlayerLeftGame() {

	if (MultiplayerSessionsSubsystem) {
		MultiplayerSessionsSubsystem->DestroySession();

	}

}

void UReturnToMainMenu::ReturnButtonClicked() {

	ReturnButton->SetIsEnabled(0);
	
	UWorld* World = GetWorld();
	if (World) {
		APlayerController* FirstPlayerController= World->GetFirstPlayerController();
		if (FirstPlayerController) {
			ABlasterCharacter* BlasterCharacter = Cast<ABlasterCharacter>(FirstPlayerController->GetPawn());
			if (BlasterCharacter) {

				BlasterCharacter->Server_LeaveGame();
				BlasterCharacter->OnLeftGame.AddDynamic(this, &UReturnToMainMenu::OnPlayerLeftGame);

			}
			else {
				ReturnButton->SetIsEnabled(1);

			}

		}


	}



}
