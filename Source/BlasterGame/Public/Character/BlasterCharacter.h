// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "TurningInPlace.h"
#include "Interfaces/InteractWCrosshairsInterface.h"
#include "Components/TimelineComponent.h"

#include "BlasterCharacter.generated.h"

class USpringArmComponent;
class UCameraComponent;
class UWidgetComponent;
class AWeapon;
class UCombatComponent;
class UAnimMontage;
class ABlasterPlayerController;
class USoundCue;
class ABlasterPlayerState;
class UBuffComponent;
class UBoxComponent;
class ULagCompensationComponent;


DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnLeftGame);



UCLASS()
class BLASTERGAME_API ABlasterCharacter : public ACharacter,public IInteractWCrosshairsInterface
{
	GENERATED_BODY()

public:
	
	ABlasterCharacter();


	virtual void Tick(float DeltaTime) override;

	
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;

	void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

	virtual void PostInitializeComponents() override;

	virtual void Jump() override;

	virtual void OnRep_ReplicatedMovement() override;

	virtual void Destroyed() override;

	UFUNCTION(BlueprintImplementableEvent)
		void ShowSniperScopeWidget(bool bShowScope);

	void UpdateHUDHealth();

	void UpdateHUDSheild();

	void UpdateHUDAmmo();

	void SpawnDefaultWeapon();

	bool bFinishedSwapping = 0;

	bool bIsFirstSwapDone = 0;

	UFUNCTION(Server, Reliable)
		void Server_LeaveGame();

	FOnLeftGame OnLeftGame;






protected:
	
	virtual void BeginPlay() override;

	

	void MoveForward(float Value);
	void MoveRight(float Value);

	void Turn(float Value);
	void LookUp(float Value);

	void EquipButtonPressed();

	void CrouchButtonPressed();

	void ReloadButtonPressed();

	void AimButtonPressed();
	void AimButtonReleased();

	void AimOffset(float DeltaTime);

	void CalculateAO_Pitch();


	void SimProxiesTurn();

	void FireButtonPressed();
	void FireButtonReleased();

	void ThrowButtonPressed();

	UFUNCTION()
		void ReceiveDamage(AActor* DamagedActor, float Damage, const class UDamageType* DamageType, class AController* InstigatedBy, AActor* DamageCauser);
	
	
	//Poll for relevent Classes and Initialize HUD


	void PollInit();

	void RotateInPlace(float DeltaTime);

	

	//HitBoxes For ServerSide Rewind

	UPROPERTY(EditAnywhere)
		UBoxComponent* head;

	UPROPERTY(EditAnywhere)
		UBoxComponent* pelvis;

	UPROPERTY(EditAnywhere)
		UBoxComponent* spine_02;

	UPROPERTY(EditAnywhere)
		UBoxComponent* spine_03;

	UPROPERTY(EditAnywhere)
		UBoxComponent* upperarm_l;

	UPROPERTY(EditAnywhere)
		UBoxComponent* upperarm_r;

	UPROPERTY(EditAnywhere)
		UBoxComponent* lowerarm_l;

	UPROPERTY(EditAnywhere)
		UBoxComponent* lowerarm_r;

	UPROPERTY(EditAnywhere)
		UBoxComponent* hand_l;

	UPROPERTY(EditAnywhere)
		UBoxComponent* hand_r;

	UPROPERTY(EditAnywhere)
		UBoxComponent* backpack;

	UPROPERTY(EditAnywhere)
		UBoxComponent* blanket;

	UPROPERTY(EditAnywhere)
		UBoxComponent* thigh_l;

	UPROPERTY(EditAnywhere)
		UBoxComponent* thigh_r;

	UPROPERTY(EditAnywhere)
		UBoxComponent* calf_l;

	UPROPERTY(EditAnywhere)
		UBoxComponent* calf_r;

	UPROPERTY(EditAnywhere)
		UBoxComponent* foot_l;

	UPROPERTY(EditAnywhere)
		UBoxComponent* foot_r;

	



private:
	UPROPERTY(VisibleAnywhere, Category = Camera)
		USpringArmComponent* CameraBoom;

	UPROPERTY(VisibleAnywhere, Category = Camera)
		UCameraComponent* FollowCamera;

	UPROPERTY(EditAnywhere,BlueprintReadOnly, Category = HUD, meta = (AllowPrivateAccess = "true"))
		UWidgetComponent* OverheadWidget;

	UPROPERTY(ReplicatedUsing= OnRep_OverlappingWeapon, VisibleAnywhere, BlueprintReadOnly, Category = Weapon, meta = (AllowPrivateAccess = "true"))
		AWeapon* OverlappingWeapon;

	UFUNCTION()
		void OnRep_OverlappingWeapon(AWeapon* LastWeapon);

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly,Category = Components, meta = (AllowPrivateAccess = "true"))
		UCombatComponent* Combat;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = Components, meta = (AllowPrivateAccess = "true"))
		UBuffComponent* Buff;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = Components, meta = (AllowPrivateAccess = "true"))
		ULagCompensationComponent* LagCompensation;


	UFUNCTION(Server,Reliable)
		void Server_EquipButtonPressed();

	float AO_Yaw = 0.f;

	float InterpAO_Yaw = 0.f;

	float AO_Pitch = 0.f;

	FRotator StartingAimRotation;

	E_TurningInPlace TurningInPlace = E_TurningInPlace::ETIP_NotTurning;

	void TurnInPlace(float DeltaTime);

	UPROPERTY(EditAnywhere, Category = Combat)
		UAnimMontage* FireWeaponMontage;

	UPROPERTY(EditAnywhere, Category = Combat)
		UAnimMontage* FireAimWeaponMontage;

	UPROPERTY(EditAnywhere, Category = Combat)
		UAnimMontage* HitReactMontage;

	UPROPERTY(EditAnywhere, Category = Combat)
		UAnimMontage* ElimMontage;

	UPROPERTY(EditAnywhere, Category = Combat)
		UAnimMontage* ReloadMontage;

	UPROPERTY(EditAnywhere, Category = Combat)
		UAnimMontage* ThrowGrenadeMontage;

	UPROPERTY(EditAnywhere, Category = Combat)
		UAnimMontage* SwapWeaponMontage;


	void HideCharacterIfCameraClose();

	UPROPERTY(EditAnywhere, Category = Combat)
		float CameraThreshold = 200.f;

	void PlayHitReactMonatge();

	bool bRotateRootBone = 0;

	float TurnThreshold = .5f;

	FRotator ProxyRotationLastFrame;

	FRotator ProxyRotation;

	float ProxyYaw = 0.f;

	float TimeSinceLastMovRep = 0.f;

	float CalculateSpeed();


	//PlayerHealth
	UPROPERTY(EditAnywhere, Category = PlayerStats)
		float MaxHealth = 100.f;

	UPROPERTY(ReplicatedUsing= OnRep_Health,VisibleAnywhere, Category = PlayerStats)
		float Health = 100.f;

	UFUNCTION()
		void OnRep_Health(float LastHealth);

	//PlayerSheild

	UPROPERTY(EditAnywhere, Category = PlayerStats)
		float MaxSheild = 100.f;

	UPROPERTY(ReplicatedUsing = OnRep_Sheild, VisibleAnywhere, Category = PlayerStats)
		float Sheild = 100.f;

	UFUNCTION()
		void OnRep_Sheild(float LastSheild);


	UPROPERTY(VisibleAnywhere, Category = Components)
		bool bElimmed = 0;

	UPROPERTY(VisibleAnywhere, Category = Components)
		ABlasterPlayerController* BlasterPlayerController;

	FTimerHandle ElimTimer;

	UPROPERTY(EditDefaultsOnly, Category = Components)
		float ElimDelay = 3.f;

	void ElimTimerFinished();

	bool bLeftGame = 0;

	
	




	//DissolveEffect

	UPROPERTY(VisibleAnywhere, Category = Components)
		UTimelineComponent* DissolveTimeline;

	UPROPERTY(EditAnywhere, Category = Components)
		UCurveFloat* DissolveCurve;


	FOnTimelineFloat DissolveTrack;

	void StartDissolve();

	UFUNCTION()
	void UpdateDissolveMaterial(float DissolveValue);

	UPROPERTY(VisibleAnywhere, Category = Components)
		UMaterialInstanceDynamic* DynamicDissolveMI;

	UPROPERTY(EditAnywhere, Category = Components)
		UMaterialInstance* DissolveMI;

	//Elim Bot

	UPROPERTY(EditAnywhere, Category = Components)
		UParticleSystem* ElimBotEffect;

	UPROPERTY(VisibleAnywhere, Category = Components)
		UParticleSystemComponent* ElimBotComponent;

	UPROPERTY(EditAnywhere, Category = Components)
		USoundCue* ElimBotSound;

	UPROPERTY(VisibleAnywhere, Category = Components)
		ABlasterPlayerState* BlasterPlayerState;

	//Grenade

	UPROPERTY(VisibleAnywhere, Category = Camera)
		UStaticMeshComponent* AttachedGrenade;

	//DefaultWeapon

	UPROPERTY(EditAnywhere, Category = Components)
		TSubclassOf<AWeapon> DefaultWeaponClass;

	float PollinTime = 0.f;
	int32 PollingInt = 0;

	bool bShouldPoll = 1;

	void StartingPollForHealth(float DeltaTime);




public:	

	void SetOverlappingWeapon(AWeapon* Weapon);

	bool IsWeaponEquipped();

	bool GetIsAiming();
	

	FORCEINLINE float GetAO_Yaw() const { return AO_Yaw; }

	FORCEINLINE float GetAO_Pitch() const { return AO_Pitch; }

	AWeapon* GetEquippedWeapon();

	FORCEINLINE E_TurningInPlace GetTurningInPlace() const { return TurningInPlace; }

	void PlayFireMonatge(bool bAiming);

	void PlayReloadMonatge();

	void PlayElimMontage();

	void PlayThrowGrenadeMonatge();

	void PlaySwapWeaponMonatge();

	FVector GetHitTarget() const;

	FORCEINLINE UCameraComponent* GetFollowCamera() const { return FollowCamera; }

	FORCEINLINE bool GetShouldRotateRootBone() const { return bRotateRootBone; }

	FORCEINLINE bool GetElimmed() const { return bElimmed; }


	void Elim(bool bPlayerLeftGame);


	UFUNCTION(NetMulticast,Reliable)
	void Multicast_Elim(bool bPlayerLeftGame);

	FORCEINLINE float GetHealth() const { return Health; }

	FORCEINLINE float GetMaxHealth() const { return MaxHealth; }

	FORCEINLINE void SetHealth(float HealAmount)  { Health = HealAmount;  }

	FORCEINLINE float GetSheild() const { return Sheild; }

	FORCEINLINE float GetMaxSheild() const { return MaxSheild; }

	FORCEINLINE void SetSheild(float HealAmount) { Sheild = HealAmount; }

	

	E_CombatState GetCombatState() const;

	UPROPERTY(Replicated)
		bool bDisableGameplay = 0;

	FORCEINLINE UCombatComponent* GetCombat() const { return Combat; }

	FORCEINLINE bool GetDisableGameplay() const { return bDisableGameplay; }

	FORCEINLINE UStaticMeshComponent* GetAttachedGrenade() const { return AttachedGrenade; }

	FORCEINLINE UBuffComponent* GetBuff() const { return Buff; }

	bool GetIsLocallyReloading();

	FORCEINLINE ULagCompensationComponent* GetLagCompensation() const { return LagCompensation; }


	UPROPERTY()
		TMap<FName,  UBoxComponent*> HitCollisionBoxes;











};
