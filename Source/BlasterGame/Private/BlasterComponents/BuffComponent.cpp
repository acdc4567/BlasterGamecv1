// Fill out your copyright notice in the Description page of Project Settings.


#include "BlasterComponents/BuffComponent.h"
#include "Character/BlasterCharacter.h"
#include "GameFramework/CharacterMovementComponent.h"

// Sets default values for this component's properties
UBuffComponent::UBuffComponent()
{
	
	PrimaryComponentTick.bCanEverTick = true;

	
}


// Called when the game starts
void UBuffComponent::BeginPlay()
{
	Super::BeginPlay();

	// ...
	
}




// Called every frame
void UBuffComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	HealRampUp(DeltaTime);

	SheildRampUp(DeltaTime);




}

void UBuffComponent::Heal(float HealAmount, float HealingTime) {
	
	bHealing = 1;

	HealingRate = HealAmount / HealingTime;
	AmountToHeal += HealAmount;






}

void UBuffComponent::ReplenishSheild(float HealAmount, float HealingTime) {

	bSheilding = 1;

	SheildingRate = HealAmount / HealingTime;
	AmountToSheild += HealAmount;




}

void UBuffComponent::BuffSpeed(float BuffBaseSpeed, float BuffCrouchSpeed, float BuffTime) {

	if (Character == nullptr)return;

	Character->GetWorldTimerManager().SetTimer(SpeedBuffTimer, this, &UBuffComponent::ResetSpeeds, BuffTime);

	if (Character->GetCharacterMovement()) {
		Character->GetCharacterMovement()->MaxWalkSpeed = BuffBaseSpeed;
		Character->GetCharacterMovement()->MaxWalkSpeedCrouched = BuffCrouchSpeed;



	}

	Multicast_SpeedBuff(BuffBaseSpeed, BuffCrouchSpeed);


}

void UBuffComponent::SetInitialSpeeds(float BaseSpeed, float CrouchSpeed) {

	InitialBaseSpeed = BaseSpeed;
	InitialCrouchSpeed = CrouchSpeed;



}

void UBuffComponent::SetInitialJumpVelocity(float JumpVelocity) {

	InitialJumpVelocity = JumpVelocity;


}


void UBuffComponent::HealRampUp(float DeltaTime) {
	if (!bHealing || !Character || Character->GetElimmed())return;

	const float HealThisFrame = HealingRate * DeltaTime;

	Character->SetHealth(FMath::Clamp(Character->GetHealth() + HealThisFrame, 0, Character->GetMaxHealth()));
	Character->UpdateHUDHealth();
	AmountToHeal -= HealThisFrame;

	if (AmountToHeal <= 0.f || Character->GetHealth() >= Character->GetMaxHealth()) {
		bHealing = 0;
		AmountToHeal = 0.f;

	}

}

void UBuffComponent::SheildRampUp(float DeltaTime) {

	if (!bSheilding || !Character || Character->GetElimmed())return;

	const float HealThisFrame = SheildingRate * DeltaTime;

	Character->SetSheild(FMath::Clamp(Character->GetSheild() + HealThisFrame, 0, Character->GetMaxSheild()));
	Character->UpdateHUDSheild();
	AmountToSheild -= HealThisFrame;

	if (AmountToSheild <= 0.f || Character->GetSheild() >= Character->GetMaxSheild()) {
		bSheilding = 0;
		AmountToSheild = 0.f;

	}

}

void UBuffComponent::ResetSpeeds() {
	if (Character == nullptr)return;

	if (!Character->GetCharacterMovement())return;

	Character->GetCharacterMovement()->MaxWalkSpeed = InitialBaseSpeed;
	Character->GetCharacterMovement()->MaxWalkSpeedCrouched = InitialCrouchSpeed;


	Multicast_SpeedBuff(InitialBaseSpeed, InitialCrouchSpeed);

}

void UBuffComponent::BuffJump(float BuffJumpVelocity, float BuffTime) {

	if (Character == nullptr)return;

	Character->GetWorldTimerManager().SetTimer(JumpBuffTimer, this, &UBuffComponent::ResetJump, BuffTime);


	if (Character->GetCharacterMovement()) {
		Character->GetCharacterMovement()->JumpZVelocity = BuffJumpVelocity;


	}
	Multicast_JumpBuff(BuffJumpVelocity);

}

void UBuffComponent::ResetJump() {


	if (Character->GetCharacterMovement()) {
		Character->GetCharacterMovement()->JumpZVelocity = InitialJumpVelocity;


	}
	Multicast_JumpBuff(InitialJumpVelocity);


}

void UBuffComponent::Multicast_JumpBuff_Implementation(float JumpSpeed) {

	if (Character&& Character->GetCharacterMovement()) {
		Character->GetCharacterMovement()->JumpZVelocity = JumpSpeed;


	}



}

void UBuffComponent::Multicast_SpeedBuff_Implementation(float BaseSpeed, float CrouchSpeed) {
	if (Character && Character->GetCharacterMovement()) {

		Character->GetCharacterMovement()->MaxWalkSpeed = BaseSpeed;
		Character->GetCharacterMovement()->MaxWalkSpeedCrouched = CrouchSpeed;


	}
	


}
