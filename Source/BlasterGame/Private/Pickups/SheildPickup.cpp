// Fill out your copyright notice in the Description page of Project Settings.


#include "Pickups/SheildPickup.h"
#include "Character/BlasterCharacter.h"
#include "BlasterComponents/BuffComponent.h"





ASheildPickup::ASheildPickup() {







}

void ASheildPickup::OnSphereOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult) {

	Super::OnSphereOverlap(OverlappedComponent, OtherActor, OtherComp, OtherBodyIndex, bFromSweep, SweepResult);

	ABlasterCharacter* BlasterCharacter = Cast<ABlasterCharacter>(OtherActor);
	if (BlasterCharacter) {
		UBuffComponent* Buff = BlasterCharacter->GetBuff();

		if (Buff) {
			//Buff->Heal(HealAmount, HealingTime);
			Buff->ReplenishSheild(HealAmount, HealingTime);

		}


	}

	Destroy();



}
