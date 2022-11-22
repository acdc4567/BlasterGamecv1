// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "HUD/BlasterHUD.h"
#include "TurningInPlace.h"
#include "CombatComponent.generated.h"

class ABlasterCharacter;
class AWeapon;
class ABlasterPlayerController;
class ABlasterHUD;
class AProjectile;














UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class BLASTERGAME_API UCombatComponent : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UCombatComponent();

	friend class ABlasterCharacter;

	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	void EquipWeapon(AWeapon* WeaponToEquip);

	void SwapWeapon();


	void Reload();

	UFUNCTION(BlueprintCallable)
		void FinishReloading();

	UFUNCTION(BlueprintCallable)
		void FinishSwap();

	UFUNCTION(BlueprintCallable)
		void FinishSwapAttachWeapons();

	void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

	void FireButtonPressed(bool bPressed);

	UFUNCTION(BlueprintCallable)
		void FinishThrowGrenade();

	UFUNCTION(BlueprintCallable)
		void LaunchGrenade();

	UFUNCTION(Server,Reliable)
		void Server_LaunchGrenade(const FVector_NetQuantize& Target);

	void PickupAmmo(E_WeaponType WeaponType, int32 AmmoAmount);

	bool bLocallyReloading = 0;







protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	void SetAiming(bool bIsAiming);

	UFUNCTION(Server, Reliable)
		void Server_SetAiming(bool bIsAiming);

	UFUNCTION()
		void OnRep_EquippedWeapon();

	UFUNCTION()
		void OnRep_SecondaryWeapon();


	void Fire();

	void FireProjectileWeapon();
	void FireHitScanWeapon();
	void FireShotgun();

	void LocalFire(const FVector_NetQuantize& TraceHitTarget);

	void LocalShotgunFire(const TArray<FVector_NetQuantize>& TraceHitTargets);



	UFUNCTION(Server,Reliable)
		void Server_Fire(const FVector_NetQuantize& TraceHitTarget);

	UFUNCTION(NetMulticast, Reliable)
		void Multicast_Fire(const FVector_NetQuantize& TraceHitTarget);

	UFUNCTION(Server, Reliable)
		void Server_ShotgunFire(const TArray<FVector_NetQuantize> & TraceHitTargets);

	UFUNCTION(NetMulticast, Reliable)
		void Multicast_ShotgunFire(const TArray<FVector_NetQuantize>& TraceHitTargets);

	void TraceUnderCrosshairs(FHitResult& TraceHitResult);

	void SetHUDCrosshairs(float DeltaTime);

	UFUNCTION(Server, Reliable)
		void Server_Reload();

	void HandleReload();

	int32 AmountToReload();

	void ThrowGrenade();

	UFUNCTION(Server, Reliable)
		void Server_ThrowGrenade();


	UPROPERTY(EditAnywhere, Category = Characters)
		TSubclassOf<AProjectile> GrenadeClass;


	void DropEquippedWeapon();

	void AttachActorToRightHand(AActor* ActorToAttach);

	void AttachActorToLeftHand(AActor* ActorToAttach);

	void AttachActorToBackpack(AActor* ActorToAttach);


	void UpdateCarriedAmmo();

	void PlayEquipWeaponSound(AWeapon* WeaponToEquip);

	void ReloadEmptyWeapon();

	void ShowAttachedGrenade(bool bShowGrenade);

	void EquipPrimaryWeapon(AWeapon* WeaponToEquip);

	void EquipSecondaryWeapon(AWeapon* WeaponToEquip);












private:
	UPROPERTY(VisibleAnywhere, Category = Characters)
		ABlasterCharacter* Character;

	UPROPERTY(VisibleAnywhere, Category = Characters)
		ABlasterPlayerController* Controller;

	UPROPERTY(VisibleAnywhere, Category = Characters)
		ABlasterHUD* HUD;


	UPROPERTY(ReplicatedUsing= OnRep_EquippedWeapon, VisibleAnywhere, Category = Weapon)
		AWeapon* EquippedWeapon;

	UPROPERTY(ReplicatedUsing = OnRep_SecondaryWeapon, VisibleAnywhere, Category = Weapon)
		AWeapon* SecondaryWeapon;


	UPROPERTY(ReplicatedUsing= OnRep_Aiming,VisibleAnywhere, Category = Weapon)
		bool bAiming = 0;
	
	bool bAimButtonPressed = 0;

	UFUNCTION()
		void OnRep_Aiming();




	UPROPERTY(EditAnywhere, Category = Characters)
		float BaseWalkSpeed = 600.f;

	UPROPERTY(EditAnywhere, Category = Characters)
		float AimWalkSpeed = 450.f;

	bool bFireButtonPressed = 0;

	FVector HitTarget;

	//HUDAnd Crosshairs


	float CrosshairVelocityFactor = 0.f;

	float CrosshairInAirFactor = 0.f;

	float CrosshairAimFactor = 0.f;

	float CrosshairShootingFactor = 0.f;

	FHUDPackage HUDPackage;


	//Aiming And FOV

	UPROPERTY(VisibleAnywhere, Category = Combat)
		float DefaultFOV;

	UPROPERTY(EditAnywhere, Category = Combat)
		float ZoomedFOV = 30.f;

	UPROPERTY(VisibleAnywhere, Category = Combat)
		float CurrentFOV = 0.f;

	UPROPERTY(EditAnywhere, Category = Combat)
		float ZoomInterpSpeed = 20.f;




	void InterpFOV(float DeltaTime);

	//AutomaticFire


	FTimerHandle FireTimer;

	

	bool bCanFire = 1;

	void StartFireTimer();

	void FireTimerFinished();

	bool CanFire();


	UPROPERTY(ReplicatedUsing=OnRep_CarriedAmmo,VisibleAnywhere, Category = Combat)
		int32 CarriedAmmo;

	UFUNCTION()
	void OnRep_CarriedAmmo();

	UPROPERTY(VisibleAnywhere, Category = Combat)
		int32 MaxCarriedAmmo = 500;





	TMap<E_WeaponType, int32> CarriedAmmoMap;

	UPROPERTY(EditAnywhere, Category = Combat)
		int32 StartingARAmmo = 130;

	UPROPERTY(EditAnywhere, Category = Combat)
		int32 StartingRocketAmmo = 13;

	UPROPERTY(EditAnywhere, Category = Combat)
		int32 StartingPistolAmmo = 50;

	UPROPERTY(EditAnywhere, Category = Combat)
		int32 StartingSMGAmmo = 50;

	UPROPERTY(EditAnywhere, Category = Combat)
		int32 StartingShotGunAmmo = 50;

	UPROPERTY(EditAnywhere, Category = Combat)
		int32 StartingSniperAmmo = 50;

	UPROPERTY(EditAnywhere, Category = Combat)
		int32 StartingGrenadeLauncherAmmo = 50;

	void InitializeCarriedAmmo();

	UPROPERTY(ReplicatedUsing = OnRep_CombatState, VisibleAnywhere, Category = Combat)
		E_CombatState CombatState=E_CombatState::ECS_UnOccupied;


	UFUNCTION()
		void OnRep_CombatState();

	void UpdateAmmoValues();





public:	
	
	bool ShouldSwapWeapons();



		
};
