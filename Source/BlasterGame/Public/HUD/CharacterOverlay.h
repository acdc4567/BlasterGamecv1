// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "CharacterOverlay.generated.h"


class UProgressBar;
class UTextBlock;
class UImage;




/**
 * 
 */
UCLASS()
class BLASTERGAME_API UCharacterOverlay : public UUserWidget
{
	GENERATED_BODY()
	
public:

	UPROPERTY(meta = (BindWidget))
		UProgressBar* HealthBar;

	UPROPERTY(meta = (BindWidget))
		UTextBlock* HealthText;

	UPROPERTY(meta = (BindWidget))
		UProgressBar* SheildBar;

	UPROPERTY(meta = (BindWidget))
		UTextBlock* SheildText;

	UPROPERTY(meta = (BindWidget))
		UTextBlock* ScoreAmount;

	UPROPERTY(meta = (BindWidget))
		UTextBlock* DefeatsAmount;

	UPROPERTY(meta = (BindWidget))
		UTextBlock* AmmoAmount;

	UPROPERTY(meta = (BindWidget))
		UTextBlock* CarriedAmmoAmount;

	UPROPERTY(meta = (BindWidget))
		UTextBlock* MatchCountdownText;

	UPROPERTY(meta = (BindWidget))
		UImage* HighPingImage;

	UPROPERTY(meta = (BindWidgetAnim),Transient)
		UWidgetAnimation* HighPingAnimation;





};
