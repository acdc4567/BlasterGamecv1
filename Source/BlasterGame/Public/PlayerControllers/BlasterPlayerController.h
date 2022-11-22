// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "BlasterPlayerController.generated.h"

class ABlasterHUD;
class UCharacterOverlay;
class ABlasterGameMode;
class UUserWidget;
class UReturnToMainMenu;




DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FHighPingDelegate, bool,bPingTooHigh);



/**
 * 
 */
UCLASS()
class BLASTERGAME_API ABlasterPlayerController : public APlayerController
{
	GENERATED_BODY()
	

public:

	



	void SetHUDHealth(float Health,float MaxHealth);

	void SetHUDSheild(float Sheild, float MaxSheild);


	void SetHUDScore(float Score);

	void SetHUDDefeats(int32 Defeats);

	void SetHUDWeaponAmmo(int32 Ammo);

	void SetHUDCarriedAmmo(int32 Ammo);
	
	void SetHUDMatchDountdown(float CountdownTime);

	void SetHUDAnnouncementDountdown(float CountdownTime);

	virtual void Tick(float DeltaTime) override;

	virtual float GetServerTime();

	virtual void ReceivedPlayer() override;

	void OnMatchStateSet(FName State);

	void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

	void HandleMatchHasStarted();

	void HandleCooldown();

	float SingleTripTime = 0.f;

	FHighPingDelegate HighPingDelegate;






protected:

	virtual void BeginPlay() override;

	virtual void OnPossess(APawn* aPawn) override;

	void SetHUDTime();

	virtual void SetupInputComponent() override;





	//Sync Time B/W Server And Client

	UFUNCTION(Server,Reliable)
		void Server_RequestServerTime(float TimeOfClientReq);

	UFUNCTION(Client, Reliable)
		void Client_ReportServerTime(float TimeOfClientReq, float TimeServerRecievedClientReq);

	float ClientServerDelta = 0.f;

	UPROPERTY(EditAnywhere, Category = PlayerStats)
		float TimeSyncFrequency = 5.f;

	float TimeSyncRunningTime = 0.f;

	void CheckTimeSync(float DeltaTime);

	void PollInit();

	UFUNCTION(Server, Reliable)
		void Server_CheckMatchState();

	UFUNCTION(Client, Reliable)
		void Client_JoinMidGame(FName StateOfMatch, float Warmup, float Match, float StartingTime,float Cooldown);

	void HighPingWarning();

	void StopHighPingWarning();

	void CheckPing(float DeltaTime);

	void ShowReturnToMainMenu();










private:

	//ReturnToMAinMenu
	UPROPERTY(EditAnywhere,Category=HUD)
	TSubclassOf<UUserWidget> ReturnToMainMenuWidget;
	
	UPROPERTY()
		UReturnToMainMenu* ReturnToMainMenu;

	bool bReturnToMainMenuOpen = 0;


	UPROPERTY(EditAnywhere, Category = PlayerStats)
		ABlasterHUD* BlasterHUD;

	UPROPERTY(VisibleAnywhere, Category = PlayerStats)
		ABlasterGameMode* BlasterGameMode;



	float MatchTime = 0.f;

	float WarmUpTime = 0.f;

	float LevelStartingTime = 0.f;

	float CooldownTime = 0.f;


	uint32 CountdownInt=0;

	UPROPERTY( ReplicatedUsing=OnRep_MatchState,VisibleAnywhere, Category = PlayerStats)
		FName MatchState;

	UFUNCTION()
		void OnRep_MatchState();

	UPROPERTY(VisibleAnywhere, Category = PlayerStats)
		UCharacterOverlay* CharacterOverlay;

	

	float HUDHealth = 0.f;
	bool bInitializeHealth = 0;

	float HUDMaxHealth = 0.f;
	float HUDSheild = 0.f;
	bool bInitializeSheild = 0;

	float HUDMaxSheild = 0.f;

	float HUDScore = 0.f;
	bool bInitializeScore = 0;

	int32 HUDDefeats = 0;
	bool bInitializeDefeats = 0;

	int32 HUDCarriedAmmo = 0;
	bool bInitializeCarriedAmmo = 0;

	int32 HUDWeaponAmmo = 0;
	bool bInitializeWeaponAmmo = 0;

	float HighPingRunningTime = 0.f;
	float HighPingDuration = 5.f;

	float CheckPingFrequency = 20.f;

	UFUNCTION(Server,Reliable)
		void Server_ReportPingStatus(bool bHighPing);


	UPROPERTY(EditAnywhere)
	float HighPingThreshold = 50.f;

	float PingAnimationRunningTime = 0.f;










};
