// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Weapons/Weapon.h"
#include "ProjectileWeapon.generated.h"

class AProjectile;







/**
 * 
 */
UCLASS()
class BLASTERGAME_API AProjectileWeapon : public AWeapon
{
	GENERATED_BODY()
public:

	virtual void Fire(const FVector& HitTarget) override;



private:

	UPROPERTY(EditAnywhere, Category = Combat)
		TSubclassOf<AProjectile> ProjectileClass;

	UPROPERTY(EditAnywhere, Category = Combat)
		TSubclassOf<AProjectile> ServerSideRewindProjectileClass;


};
