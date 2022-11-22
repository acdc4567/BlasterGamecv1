// Fill out your copyright notice in the Description page of Project Settings.


#include "PlayerControllers/BlasterPlayerController.h"
#include "HUD/BlasterHUD.h"
#include "Components/ProgressBar.h"
#include "Components/TextBlock.h"
#include "Character/BlasterCharacter.h"
#include "Net/UnrealNetwork.h"
#include "GameModes/BlasterGameMode.h"
#include "HUD/Announcement.h"
#include "Kismet/GameplayStatics.h"
#include "BlasterComponents/CombatComponent.h"
#include "GameStates/BlasterGameState.h"
#include "PlayerStates/BlasterPlayerState.h"
#include "Components/Image.h"
#include "HUD/CharacterOverlay.h"
#include "HUD/ReturnToMainMenu.h"



void ABlasterPlayerController::SetupInputComponent() {

	Super::SetupInputComponent();
	if (InputComponent == nullptr)return;

	InputComponent->BindAction("Quit", IE_Pressed, this, &ABlasterPlayerController::ShowReturnToMainMenu);



}

void ABlasterPlayerController::SetHUDHealth(float Health, float MaxHealth) {

	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;

	bool bHUDValid = BlasterHUD && BlasterHUD->CharacterOverlay && BlasterHUD->CharacterOverlay->HealthBar && BlasterHUD->CharacterOverlay->HealthText;
	if (bHUDValid) {

		const float HealthPercent = Health / MaxHealth;
		BlasterHUD->CharacterOverlay->HealthBar->SetPercent(HealthPercent);
		FString HealthText = FString::Printf(TEXT("%d / %d"), FMath::CeilToInt(Health), FMath::CeilToInt(MaxHealth));
		BlasterHUD->CharacterOverlay->HealthText->SetText(FText::FromString(HealthText));
	}
	else {
		bInitializeHealth = 1;
		HUDHealth = Health;
		HUDMaxHealth = MaxHealth;


	}



}

void ABlasterPlayerController::SetHUDSheild(float Sheild, float MaxSheild) {
	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;

	bool bHUDValid = BlasterHUD && BlasterHUD->CharacterOverlay && BlasterHUD->CharacterOverlay->SheildBar && BlasterHUD->CharacterOverlay->SheildText;
	if (bHUDValid) {

		const float SheildPercent = Sheild / MaxSheild;
		BlasterHUD->CharacterOverlay->SheildBar->SetPercent(SheildPercent);
		FString SheildText = FString::Printf(TEXT("%d / %d"), FMath::CeilToInt(Sheild), FMath::CeilToInt(MaxSheild));
		BlasterHUD->CharacterOverlay->SheildText->SetText(FText::FromString(SheildText	));
	}
	else {
		bInitializeSheild = 1;
		HUDSheild = Sheild;
		HUDMaxSheild = MaxSheild;


	}





}

void ABlasterPlayerController::SetHUDScore(float Score) {

	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;

	bool bHUDValid = BlasterHUD && BlasterHUD->CharacterOverlay && BlasterHUD->CharacterOverlay->ScoreAmount ;
	if (bHUDValid) {

		
		FString ScoreText = FString::Printf(TEXT("%d "), FMath::CeilToInt(Score));
		BlasterHUD->CharacterOverlay->ScoreAmount->SetText(FText::FromString(ScoreText));
	}
	else {
		bInitializeScore = 1;
		HUDScore = Score;
	}

}

void ABlasterPlayerController::SetHUDDefeats(int32 Defeats) {


	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;

	bool bHUDValid = BlasterHUD && BlasterHUD->CharacterOverlay && BlasterHUD->CharacterOverlay->DefeatsAmount;
	if (bHUDValid) {


		FString ScoreText = FString::Printf(TEXT("%d "), Defeats);
		BlasterHUD->CharacterOverlay->DefeatsAmount->SetText(FText::FromString(ScoreText));
	}
	else {
		bInitializeDefeats = 1;
		HUDDefeats = Defeats;
	}



}

void ABlasterPlayerController::SetHUDWeaponAmmo(int32 Ammo) {

	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;

	bool bHUDValid = BlasterHUD && BlasterHUD->CharacterOverlay && BlasterHUD->CharacterOverlay->AmmoAmount;
	if (bHUDValid) {


		FString ScoreText = FString::Printf(TEXT("%d "), Ammo);
		BlasterHUD->CharacterOverlay->AmmoAmount->SetText(FText::FromString(ScoreText));
	}
	else {
		bInitializeWeaponAmmo = 1;
		HUDWeaponAmmo = Ammo;
	}




}

void ABlasterPlayerController::SetHUDCarriedAmmo(int32 Ammo) {
	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;

	bool bHUDValid = BlasterHUD && BlasterHUD->CharacterOverlay && BlasterHUD->CharacterOverlay->CarriedAmmoAmount;
	if (bHUDValid) {


		FString ScoreText = FString::Printf(TEXT("%d "), Ammo);
		BlasterHUD->CharacterOverlay->CarriedAmmoAmount->SetText(FText::FromString(ScoreText));
	}
	else {
		bInitializeCarriedAmmo = 1;
		HUDCarriedAmmo = Ammo;
	}


}

void ABlasterPlayerController::SetHUDMatchDountdown(float CountdownTime) {

	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;

	bool bHUDValid = BlasterHUD && BlasterHUD->CharacterOverlay && BlasterHUD->CharacterOverlay->MatchCountdownText;
	if (bHUDValid) {

		if (CountdownTime<0.f) {
			BlasterHUD->CharacterOverlay->MatchCountdownText->SetText(FText());
			return;
		}

		int32 Minutes = FMath::FloorToInt(CountdownTime / 60.f);
		int32 Seconds = CountdownTime - Minutes * 60;



		FString CountdownText = FString::Printf(TEXT("%02d : %02d "), Minutes, Seconds);
		BlasterHUD->CharacterOverlay->MatchCountdownText->SetText(FText::FromString(CountdownText));
	}




}

void ABlasterPlayerController::SetHUDAnnouncementDountdown(float CountdownTime) {

	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;

	bool bHUDValid = BlasterHUD && BlasterHUD->Announcement && BlasterHUD->Announcement->WarmupTime;
	if (bHUDValid) {
		if (CountdownTime < 0.f) {
			BlasterHUD->Announcement->WarmupTime->SetText(FText());
			return;
		}

		int32 Minutes = FMath::FloorToInt(CountdownTime / 60.f);
		int32 Seconds = CountdownTime - Minutes * 60;



		FString CountdownText = FString::Printf(TEXT("%02d : %02d "), Minutes, Seconds);
		BlasterHUD->Announcement->WarmupTime->SetText(FText::FromString(CountdownText));
	}



}

void ABlasterPlayerController::Tick(float DeltaTime) {

	Super::Tick(DeltaTime);


	SetHUDTime();

	CheckTimeSync(DeltaTime);
	
	PollInit();

	CheckPing(DeltaTime);




}



void ABlasterPlayerController::BeginPlay() {

	Super::BeginPlay();

	


	BlasterHUD = Cast<ABlasterHUD>(GetHUD());
	Server_CheckMatchState();
	


}

void ABlasterPlayerController::OnPossess(APawn* aPawn) {

	Super::OnPossess(aPawn);

	ABlasterCharacter* BlasterCharacter = Cast<ABlasterCharacter>(aPawn);
	if (BlasterCharacter) {
		SetHUDHealth(BlasterCharacter->GetHealth(), BlasterCharacter->GetMaxHealth());
	}



}

void ABlasterPlayerController::SetHUDTime() {

	float TimeLeft = 0.f;
	if (MatchState == MatchState::WaitingToStart) {
		TimeLeft = WarmUpTime - GetServerTime() + LevelStartingTime;
	}
	else if (MatchState == MatchState::InProgress) {
		TimeLeft = WarmUpTime +MatchTime - GetServerTime() + LevelStartingTime;
	}
	else if (MatchState == MatchState::Cooldown) {
		TimeLeft = CooldownTime+ WarmUpTime + MatchTime - GetServerTime() + LevelStartingTime;
	}



	uint32 SecondsLeft = FMath::CeilToInt(TimeLeft);

	if (HasAuthority()) {
		if (BlasterGameMode == nullptr) {
			BlasterGameMode = Cast<ABlasterGameMode>(UGameplayStatics::GetGameMode(this));
			LevelStartingTime = BlasterGameMode->LevelStartingTime;
		}
		BlasterGameMode = BlasterGameMode == nullptr ? Cast<ABlasterGameMode>(UGameplayStatics::GetGameMode(this)) : BlasterGameMode;
		if (BlasterGameMode) {
			SecondsLeft = FMath::CeilToInt(BlasterGameMode->GetCountdownTime() + LevelStartingTime);
		}
	}








	if (CountdownInt != SecondsLeft) {
		if (MatchState == MatchState::WaitingToStart|| MatchState == MatchState::Cooldown) {
			SetHUDAnnouncementDountdown(TimeLeft);


		}
		if (MatchState == MatchState::InProgress) {

			SetHUDMatchDountdown(TimeLeft);


		}

	}


	CountdownInt = SecondsLeft;



}

void ABlasterPlayerController::CheckTimeSync(float DeltaTime) {
	TimeSyncRunningTime += DeltaTime;

	if (IsLocalController() && TimeSyncRunningTime > TimeSyncFrequency) {
		Server_RequestServerTime(GetWorld()->GetTimeSeconds());

		TimeSyncRunningTime = 0.f;

	}



}

void ABlasterPlayerController::PollInit() {
	if (CharacterOverlay == nullptr) {

		if (BlasterHUD && BlasterHUD->CharacterOverlay) {
			CharacterOverlay = BlasterHUD->CharacterOverlay;
			if (CharacterOverlay) {
				if (bInitializeHealth) SetHUDHealth(HUDHealth, HUDMaxHealth);
				if (bInitializeSheild) SetHUDSheild(HUDSheild, HUDMaxSheild);
				if (bInitializeScore) SetHUDScore(HUDScore);
				if (bInitializeDefeats) SetHUDDefeats(HUDDefeats);
				if (bInitializeCarriedAmmo) SetHUDCarriedAmmo(HUDCarriedAmmo);
				if (bInitializeWeaponAmmo) SetHUDWeaponAmmo(HUDWeaponAmmo);

			}

		}

	}



}

void ABlasterPlayerController::HighPingWarning() {
	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;

	bool bHUDValid = BlasterHUD && BlasterHUD->CharacterOverlay && BlasterHUD->CharacterOverlay->HighPingImage && BlasterHUD->CharacterOverlay->HighPingAnimation;
	if (bHUDValid) {
		BlasterHUD->CharacterOverlay->HighPingImage->SetOpacity(1.f);
		BlasterHUD->CharacterOverlay->PlayAnimation(BlasterHUD->CharacterOverlay->HighPingAnimation, 0.f, 5);


	}



}

void ABlasterPlayerController::StopHighPingWarning() {

	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;

	bool bHUDValid = BlasterHUD && BlasterHUD->CharacterOverlay && BlasterHUD->CharacterOverlay->HighPingImage && BlasterHUD->CharacterOverlay->HighPingAnimation;
	if (bHUDValid) {
		BlasterHUD->CharacterOverlay->HighPingImage->SetOpacity(0.f);
		if (BlasterHUD->CharacterOverlay->IsAnimationPlaying(BlasterHUD->CharacterOverlay->HighPingAnimation)) {

			BlasterHUD->CharacterOverlay->StopAnimation(BlasterHUD->CharacterOverlay->HighPingAnimation);

		}
		


	}





}

void ABlasterPlayerController::CheckPing(float DeltaTime) {

	HighPingRunningTime += DeltaTime;
	if (HighPingRunningTime > CheckPingFrequency) {
		PlayerState = PlayerState == nullptr ? GetPlayerState<APlayerState>() : PlayerState;
		if (PlayerState) {
			if (PlayerState->GetPingInMilliseconds() * 4 > HighPingThreshold) {
				HighPingWarning();
				PingAnimationRunningTime = 0.f;
				Server_ReportPingStatus(1);
			}
			else {
				Server_ReportPingStatus(0);

			}


		}
		HighPingRunningTime = 0.f;


	}
	bool bHighPingAnimationPlaying = BlasterHUD && BlasterHUD->CharacterOverlay && BlasterHUD->CharacterOverlay->HighPingAnimation && BlasterHUD->CharacterOverlay->IsAnimationPlaying(BlasterHUD->CharacterOverlay->HighPingAnimation);
	if (bHighPingAnimationPlaying) {

		PingAnimationRunningTime += DeltaTime;
		if (PingAnimationRunningTime > HighPingDuration) {
			StopHighPingWarning();


		}

	}



}

void ABlasterPlayerController::ShowReturnToMainMenu() {

	if (ReturnToMainMenuWidget == nullptr)return;
	if (ReturnToMainMenu == nullptr) {
		ReturnToMainMenu = CreateWidget<UReturnToMainMenu>(this, ReturnToMainMenuWidget);
	}
	if (ReturnToMainMenu) {
		bReturnToMainMenuOpen = !bReturnToMainMenuOpen;
		if (bReturnToMainMenuOpen) {
			ReturnToMainMenu->MenuSetup();

		}
		else {
			ReturnToMainMenu->MenuTeardown();
		}

	}

}

void ABlasterPlayerController::Server_CheckMatchState_Implementation() {

	ABlasterGameMode* GameMode = Cast<ABlasterGameMode>(UGameplayStatics::GetGameMode(this));
	if (GameMode) {
		WarmUpTime = GameMode->WarmUpTime;
		MatchTime = GameMode->MatchTime;
		CooldownTime = GameMode->CooldownTime;
		LevelStartingTime = GameMode->LevelStartingTime;
		MatchState = GameMode->GetMatchState();

		Client_JoinMidGame(MatchState, WarmUpTime, MatchTime, LevelStartingTime,CooldownTime);

		


	}





}

void ABlasterPlayerController::Client_JoinMidGame_Implementation(FName StateOfMatch,float Warmup,float Match,float StartingTime, float Cooldown) {
	WarmUpTime = Warmup;
	
	LevelStartingTime = StartingTime;
	MatchTime = Match;
	MatchState = StateOfMatch;
	CooldownTime = Cooldown;
	OnMatchStateSet(MatchState);
	if (BlasterHUD&& MatchState == MatchState::WaitingToStart) {
		BlasterHUD->AddAnnouncement();
	}

}

void ABlasterPlayerController::OnRep_MatchState() {

	if (MatchState == MatchState::InProgress) {

		HandleMatchHasStarted();

	}
	else if (MatchState == MatchState::Cooldown) {
		HandleCooldown();
	}


}

void ABlasterPlayerController::Server_ReportPingStatus_Implementation(bool bHighPing) {
	HighPingDelegate.Broadcast(bHighPing);


}

void ABlasterPlayerController::Server_RequestServerTime_Implementation(float TimeOfClientReq) {
	float ServerTimeOfReciept= GetWorld()->GetTimeSeconds();
	Client_ReportServerTime(TimeOfClientReq, ServerTimeOfReciept);




}

void ABlasterPlayerController::Client_ReportServerTime_Implementation(float TimeOfClientReq, float TimeServerRecievedClientReq) {
	float RoundTripTime = GetWorld()->GetTimeSeconds() - TimeOfClientReq;
	SingleTripTime = .5f * RoundTripTime;
	float CurrentServerTime = TimeServerRecievedClientReq + SingleTripTime;
	
	ClientServerDelta = CurrentServerTime - GetWorld()->GetTimeSeconds();






}




float ABlasterPlayerController::GetServerTime() {
	if (HasAuthority()) {

		return GetWorld()->GetTimeSeconds();
	}
	else {
		return GetWorld()->GetTimeSeconds() + ClientServerDelta;
	}

	return 0.f;
}

void ABlasterPlayerController::ReceivedPlayer() {

	Super::ReceivedPlayer();

	if (IsLocalController()) {

		Server_RequestServerTime(GetWorld()->GetTimeSeconds());



	}


}

void ABlasterPlayerController::OnMatchStateSet(FName State) {
	MatchState = State;
	if (MatchState == MatchState::InProgress) {
		HandleMatchHasStarted();
	}
	else if (MatchState == MatchState::Cooldown) {
		HandleCooldown();
	}

	



}

void ABlasterPlayerController::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const {

	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(ABlasterPlayerController, MatchState);





}

void ABlasterPlayerController::HandleMatchHasStarted() {


	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;
	if (BlasterHUD) {
		if (BlasterHUD->CharacterOverlay == nullptr) BlasterHUD->AddCharacterOverlay();

		
	}
	if (BlasterHUD && BlasterHUD->Announcement) {
		BlasterHUD->Announcement->SetVisibility(ESlateVisibility::Hidden);
	}




}

void ABlasterPlayerController::HandleCooldown() {

	BlasterHUD = BlasterHUD == nullptr ? Cast<ABlasterHUD>(GetHUD()) : BlasterHUD;
	if (BlasterHUD) {
		BlasterHUD->CharacterOverlay->RemoveFromParent();
		bool bHUDValid = BlasterHUD->Announcement && BlasterHUD->Announcement->AnnouncementText && BlasterHUD->Announcement->InfoText;
		if (bHUDValid) {
			BlasterHUD->Announcement->SetVisibility(ESlateVisibility::Visible);
			FString AnnouncementText("New Match Starts In : ");
			BlasterHUD->Announcement->AnnouncementText->SetText(FText::FromString(AnnouncementText));
			ABlasterGameState* BlasterGameState = Cast<ABlasterGameState>( UGameplayStatics::GetGameState(this));
			ABlasterPlayerState* BlasterPlayerState = GetPlayerState<ABlasterPlayerState>();


			if (BlasterGameState&& BlasterPlayerState) {

				TArray<ABlasterPlayerState* > TopPlayers = BlasterGameState->TopScoringPlayers;
				FString InfoTextString;

				if (TopPlayers.Num() == 0) {
					InfoTextString = FString("There Is No Winner");

				}
				else if (TopPlayers.Num() == 1&&TopPlayers[0]==BlasterPlayerState) {
					InfoTextString = FString("You are the Winner!!!");

				}
				else if (TopPlayers.Num() == 1 ) {
					InfoTextString = FString::Printf(TEXT("Winner : \n%s"),*TopPlayers[0]->GetPlayerName());

				}
				else if (TopPlayers.Num() > 1) {
					InfoTextString = FString("Winners : \n");
					for (auto TiedPlayer : TopPlayers) {
						InfoTextString.Append(FString::Printf(TEXT("%s\n"), *TiedPlayer->GetPlayerName()));


					}
				}
				BlasterHUD->Announcement->InfoText->SetText(FText::FromString(InfoTextString));

			}
			
			
			

		}
	}
	
	ABlasterCharacter* BlasterCharacter = Cast<ABlasterCharacter>(GetPawn());
	if (BlasterCharacter && BlasterCharacter->GetCombat()) {

		BlasterCharacter->bDisableGameplay = 1;
		BlasterCharacter->GetCombat()->FireButtonPressed(0);
	}



}








