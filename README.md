# Bugmageddon Player Function

This repository shows how the player loads its relevant components and the various systems controlling the player.

All scripts and code architecture were made by the author of this document, William Wasenius.

---

## The Problem That Needed Solving

The Player Assembly System in Bugmageddon presents a structural challenge that is not encountered in games with fixed player characters. In a typical game, the player character is a single pre-built entity with all visual, mechanical, and audio properties bundled together. In Bugmageddon, the player character is instead assembled at runtime from independently chosen parts, each of which must function as a standalone entity in its own right.

This modularity is a deliberate design requirement. Weapons, for example, are not exclusive to the player; the same weapon scripts are used by non-player characters elsewhere in the game. This means each component cannot be tightly coupled to the player. It must be capable of operating independently regardless of what it is attached to. The same applies to systems such as the health and destruction logic, which is shared between the player, enemies, and destructible environment objects.

This introduces two core problems:

1. **Persistence between scenes.** The player's configuration of mech and weapons must be stored somewhere that survives the transition between the loadout screen and the mission scene, since all objects in a scene are destroyed when a new one loads.
2. **Runtime references.** Once the chosen components are instantiated in the mission scene, they cannot rely on pre-existing connections to one another, as each is a fresh clone with no awareness of the other parts. All references between components must therefore be established at runtime, after instantiation.

---

## The Solution

Figure 1 illustrates the structure of the Player Assembly System and the relationships between its components, such as which GameObjects are nested within others and which scripts/components feed information to other scripts/components.

<div align="center">
  <img width="1180" alt="Player Assembly System diagram" src="https://github.com/user-attachments/assets/9c0030a0-7649-44a1-8654-66e1e9cd08cb" />
  <br />
  <i>Figure 1. The connection between the Player Assembly System's GameObjects and scripts.</i>
</div>

### The Selection Manager

The Selection Manager is a persistent singleton that stores the player's chosen mech and weapon indices as integer values. It is implemented using the singleton pattern, meaning only one instance can exist at any time. If a second instance is created, it immediately destroys itself.

The Selection Manager persists across scene transitions, carrying the player's choices from the loadout screen into each mission. It does not write data to disk, so selections reset between play sessions.

### The Player Container

The Player Container hosts the `PlayerController` script, which is the central coordinator of the player GameObject and the Player Assembly System.

On startup, it queries the Selection Manager for the chosen mech index and retrieves the corresponding `MechStatsSO` scriptable object. From this, it sets the mech's health and armor values on the `DestructibleScript` component, and instantiates the mech's 3D model as a child object. Once the model is loaded, it retrieves the `FetchMechComponentsScript` from the newly created child hierarchy, which provides references to the mech's key components such as the animator, aim pivot, weapon hardpoints, and animation rig.

The `PlayerController` then handles all player input: movement, weapon firing, the stomp ability, and toggling weapon aim elevation.

### The Mech Container

Each mech prefab contains a `FetchMechComponentsScript`, which is responsible for locating and exposing the mech's internal components to the rest of the system. On startup, it searches the scene for named objects, specifically the torso parts, aim pivot, weapon hardpoints, and animator, then stores references to them. These references are then accessed by the `PlayerController` to drive movement, animation, and aiming logic.

This design decouples the player logic from the specific internal structure of each mech model: the `PlayerController` does not need to know how a given mech is built internally, only that `FetchMechComponentsScript` will provide the expected references.

### The Armature

The "skeleton" of the model, the armature contains all systems that are connected to animations, such as leaving behind footprints after each step. All the scripts attached to the armature are universal scripts, which are reused for other entities such as enemies.

The armature's role in playing and syncing the animations with the `PlayerController` script, and in containing the weapons container, is worth noting.

### The Weapons Container

Weapons are loaded into the scene by the `LoadWeapon` script, which resides on the mech's armature. On startup, it reads the selected weapon indices from the Selection Manager and instantiates the corresponding prefabs at the mech's left and right hardpoint positions.

Each weapon prefab carries a `WeaponHandler` script, which on awakening searches upward through its parent hierarchy for the `PlayerController` and registers itself by calling `AssignWeapon()`. The first weapon to register takes the right slot, the second takes the left. This self-registration pattern means the `PlayerController` does not need to know in advance which weapons are loaded, as the weapons announce themselves.

The `WeaponHandler` manages all firing logic independently: fire rate cooldowns, heat generation and overheat lockout, ramping fire rate, and charge-shot accumulation. The `PlayerController` receives player input and calls `TryFire()` or `ReleaseChargeShot()` on the relevant `WeaponHandler`, but delegates all firing logic to the weapon itself.

Each `WeaponHandler` references a `WeaponStatsSO` scriptable object for its numerical parameters, and a separate `ProjectileStatsSO` (accessed through the weapon stats) defines the properties of the projectile fired, such as damage, velocity, and the prefab to spawn.
