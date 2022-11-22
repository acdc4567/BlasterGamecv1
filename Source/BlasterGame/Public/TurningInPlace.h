#pragma once


UENUM(BlueprintType)
enum class E_TurningInPlace :uint8 {
	ETIP_Left UMETA(DisplayName = "Turning Left"),
	ETIP_Right UMETA(DisplayName = "Turning Right"),
	ETIP_NotTurning UMETA(DisplayName = "Not Turning"),

	ETIP_MAX UMETA(DisplayName = "DefaultMAX")

};

#define ECC_SkeletalMesh ECollisionChannel::ECC_GameTraceChannel1

#define ECC_HitBox ECollisionChannel::ECC_GameTraceChannel2

#define TRACE_LENGTH 80000.f

#define CUSTOM_DEPTH_PURPLE 250

#define CUSTOM_DEPTH_BLUE 251

#define CUSTOM_DEPTH_TAN 252


UENUM(BlueprintType)
enum class E_WeaponType :uint8 {
	EWT_AssaultRifle UMETA(DisplayName = "Assault Rifle"),
	EWT_RocketLauncher UMETA(DisplayName = "Rocket Launcher"),
	EWT_Pistol UMETA(DisplayName = "Pistol"),
	EWT_SubmachineGun UMETA(DisplayName = "Submachine Gun"),
	EWT_ShotGun UMETA(DisplayName = "Shot Gun"),
	EWT_SniperRifle UMETA(DisplayName = "Sniper Rifle"),
	EWT_GrenadeLauncher UMETA(DisplayName = "Grenade Launcher"),

	EWT_MAX UMETA(DisplayName = "DefaultMAX")

};

UENUM(BlueprintType)
enum class E_CombatState :uint8 {
	ECS_UnOccupied UMETA(DisplayName = "UnOccupied"),
	ECS_Reloading UMETA(DisplayName = "Reloading"),
	ECS_ThrowingGrenade UMETA(DisplayName = "ThrowingGrenade"),
	ECS_SwappingWeapons UMETA(DisplayName = "Swapping Weapons"),

	ECS_MAX UMETA(DisplayName = "DefaultMAX")

};