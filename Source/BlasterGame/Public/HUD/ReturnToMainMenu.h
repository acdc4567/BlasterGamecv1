// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "ReturnToMainMenu.generated.h"

class UButton;
class UMultiplayerSessionsSubsystem;
class APlayerController;





/**
 * 
 */
UCLASS()
class BLASTERGAME_API UReturnToMainMenu : public UUserWidget
{
	GENERATED_BODY()
	
public:
	void MenuSetup();

	void MenuTeardown();


protected:

	virtual bool Initialize() override;

	UFUNCTION()
		void OnDestroySession(bool bWasSuccessful);

	UFUNCTION()
		void OnPlayerLeftGame();






private:

	UPROPERTY(meta = (BindWidget))
		UButton* ReturnButton;

	UFUNCTION()
		void ReturnButtonClicked();


	UPROPERTY()
		UMultiplayerSessionsSubsystem* MultiplayerSessionsSubsystem;

	UPROPERTY()
		APlayerController* PlayerController;






};
