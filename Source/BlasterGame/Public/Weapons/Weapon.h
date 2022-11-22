// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "TurningInPlace.h"

#include "Weapon.generated.h"

class USphereComponent;
class UWidgetComponent;
class UAnimationAsset;
class ACasing;
class UTexture2D;
class ABlasterCharacter;
class ABlasterPlayerController;
class USoundCue;





UENUM(BlueprintType)
enum class E_WeaponState :uint8 {
	EWS_Initial UMETA(DisplayName = "InitialState"),
	EWS_Equipped UMETA(DisplayName = "Equipped"),
	EWS_EquippedSecondary UMETA(DisplayName = "EquippedSecondary"),
	EWS_Dropped UMETA(DisplayName = "Dropped"),

	EWS_MAX UMETA(DisplayName = "DefaultMAX")

};

UENUM(BlueprintType)
enum class E_FireType :uint8 {
	EFT_HitScan UMETA(DisplayName = "HitScanWeapon"),
	EFT_Projectile UMETA(DisplayName = "ProjectileWeapon"),
	EFT_Shotgun UMETA(DisplayName = "ShotgunWeapon"),

	EFT_MAX UMETA(DisplayName = "DefaultMAX")

};





UCLASS()
class BLASTERGAME_API AWeapon : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	AWeapon();

	// Called every frame
	virtual void Tick(float DeltaTime) override;

	void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

	void SetHUDAmmo();

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = WeaponProperties)
		USoundCue* EquipSound;


	//Enable or Disable Custom Depth

	void EnableCustomDepth(bool bEnable);


	UPROPERTY(EditAnywhere)
		E_FireType FireType;

	UPROPERTY(EditAnywhere, Category = WeaponScatter)
		bool bUseScatter = 0;

	FVector TraceEndWithScatter(const FVector& HitTarget);


protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

	virtual void OnWeaponStateSet();

	virtual void OnEquipped();

	virtual void OnDropped();

	virtual void OnEquippedSecondary();





	UFUNCTION()
		virtual void OnSphereOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);

	UFUNCTION()
		virtual void OnSphereEndOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex);

	virtual void OnRep_Owner() override;

	UFUNCTION(BlueprintImplementableEvent)
		void ChangePhysicsAssetx();

	UFUNCTION(BlueprintImplementableEvent)
		void ChangePhysicsAssetx1();



	//Trace End With Scatter

	UPROPERTY(EditAnywhere, Category = WeaponScatter)
		float DistanceToSphere = 800.f;

	UPROPERTY(EditAnywhere, Category = WeaponScatter)
		float SphereRadius = 75.f;

	UPROPERTY(EditAnywhere, Category = Camera)
		float Damage = 5.f;

	UPROPERTY( Replicated,EditAnywhere, Category = Camera)
		bool bUseServerSideRewind = 0;

	UPROPERTY(VisibleAnywhere, Category = WeaponProperties)
		ABlasterCharacter* BlasterCharacter;

	UPROPERTY(VisibleAnywhere, Category = WeaponProperties)
		ABlasterPlayerController* BlasterPlayerController;

	UFUNCTION()
		void OnPingTooHigh(bool bPingTooHi);




private:

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = WeaponProperties, meta = (AllowPrivateAccess = "true"))
		USkeletalMeshComponent* WeaponMesh;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = WeaponProperties, meta = (AllowPrivateAccess = "true"))
		USphereComponent* AreaSphere;

	UPROPERTY(ReplicatedUsing = OnRep_WeaponState,VisibleAnywhere, BlueprintReadOnly, Category = WeaponProperties, meta = (AllowPrivateAccess = "true"))
		E_WeaponState WeaponState=E_WeaponState::EWS_Initial;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = HUD, meta = (AllowPrivateAccess = "true"))
		UWidgetComponent* PickupWidget;


	UFUNCTION()
		void OnRep_WeaponState();


	UPROPERTY(EditAnywhere, Category = WeaponProperties)
		UAnimationAsset* FireAnimation;

	UPROPERTY(EditAnywhere, Category = WeaponProperties)
		TSubclassOf<ACasing> CasingClass;

	UPROPERTY(EditAnywhere, Category = WeaponProperties)
		int32 Ammo=30;

	UPROPERTY(EditAnywhere, Category = WeaponProperties)
		int32 MagCapacity = 30;
	
	UFUNCTION(Client, Reliable)
		void Client_UpdateAmmo(int32 ServerAmmo);


	UFUNCTION(Client, Reliable)
		void Client_AddAmmo(int32 AmmoToAdd);

	void SpendRound();

	

	

	UPROPERTY(EditAnywhere, Category = WeaponProperties)
		E_WeaponType WeaponType;

	//Number of Unprocessed Server Requests
	int32 Sequence = 0;



	//ZoomedFOV While Aiming

	UPROPERTY(EditAnywhere, Category = WeaponProperties)
		float ZoomedFOV = 30.f;

	UPROPERTY(EditAnywhere, Category = WeaponProperties)
		float ZoomInterpSpeed = 20.f;

	




public:	

	bool bDestroyWeapon = 0;

	void ShowPickupWidget(bool bShowWidget);

	void SetWeaponState(E_WeaponState WeaponStatex);

	FORCEINLINE USphereComponent* GetAreaSphere() const { return AreaSphere; }

	FORCEINLINE USkeletalMeshComponent* GetWeaponMesh() const { return WeaponMesh; }

	virtual void Fire(const FVector& HitTarget);

	//Textures For WeaponCrosshairs

	UPROPERTY(EditAnywhere, Category = Crosshairs)
		UTexture2D* CrosshairsCenter;

	UPROPERTY(EditAnywhere, Category = Crosshairs)
		UTexture2D* CrosshairsLeft;

	UPROPERTY(EditAnywhere, Category = Crosshairs)
		UTexture2D* CrosshairsRight;

	UPROPERTY(EditAnywhere, Category = Crosshairs)
		UTexture2D* CrosshairsTop;

	UPROPERTY(EditAnywhere, Category = Crosshairs)
		UTexture2D* CrosshairsBottom;

	FORCEINLINE float GetZoomedFOV() const { return ZoomedFOV; }

	FORCEINLINE float GetZoomInterpSpeed() const { return ZoomInterpSpeed; }

	UPROPERTY(EditAnywhere, Category = WeaponProperties)
		float FireDelay = .1f;

	UPROPERTY(EditAnywhere, Category = WeaponProperties)
		bool bAutomatic = 1;

	void Dropped();

	bool IsEmpty();

	FORCEINLINE E_WeaponType GetWeaponType() const { return WeaponType; }

	FORCEINLINE int32 GetAmmo() const { return Ammo; }
	FORCEINLINE int32 GetMagCapacity() const { return MagCapacity; }

	void AddAmmo(int32 AmmoToAdd);

	FORCEINLINE bool GetIsFull() const { return Ammo == MagCapacity; }

	FORCEINLINE float GetDamage() const { return Damage; }








};
