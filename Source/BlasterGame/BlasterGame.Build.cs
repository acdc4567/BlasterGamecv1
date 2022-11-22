// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

public class BlasterGame : ModuleRules
{
	public BlasterGame(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "HeadMountedDisplay","UMG" ,"Niagara", "MultiplayerSessions", "OnlineSubsystem", "OnlineSubsystemSteam"});

        PublicIncludePaths.AddRange(new string[] { 
		"BlasterGame/Public/Character",
        "BlasterGame/Public/GameModes",
        "BlasterGame/Public/HUD",
        "BlasterGame/Public/Weapons",
        "BlasterGame/Public/BlasterComponents",
        "BlasterGame/Public/PlayerControllers",
        "BlasterGame/Public/Interfaces",
        "BlasterGame/Public/PlayerStates",
        "BlasterGame/Public/GameStates",
        "BlasterGame/Public/Pickups"
        });


    }
}
