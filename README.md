# Gameplay Overview

## Core Objective
Build (or conquer) a set number of **Cities** (typically 3–8, chosen at setup) and hold them for at least one full round to win. The game emphasizes long-term development over pure aggression.

## Game Setup
- **Map**: Mix of land and water. Players choose land/water ratio and resource density.
- **Players**: Assign colors. Computer opponents can be Aggressive, Defensive, or Passive.
- **Resources**: Randomly placed on territories. Key types include:
  - **Gold** (basic economy).
  - **Horses** (cavalry/mobility bonuses).
  - **Weapons** (infantry/firepower).
  - **Boats** (naval transport and sea attacks).
- Initial territory selection (like Risk): Players claim starting provinces.

## Turn Structure (One Year per Full Round)
Each turn/phase is handled sequentially by players:

1. **Production** — Generate resources based on controlled territories and improvements.
2. **Trade** (multiplayer option) — Negotiate/exchange resources with others.
3. **Shipment / Movement** — Transport stockpiles or move units (boats enable sea movement).
4. **Conquest / Attack Phase** — Plan and execute attacks (usually limited to 2–3 per turn).
   - Attacks are only possible against **adjacent** territories.
   - Combat is resolved by comparing **force points**:
     - Base: 1 point per territory.
     - Bonuses: Adjacent territories (+1 each), Horses (+1), Weapons (+3), Boats (+2), Cities (+2), etc.
     - Defender often gets terrain/adjacent support advantages.
     - Outcomes involve some randomness (player-settable odds).
5. **Development / Construction** — Spend stockpiled resources to:
   - Build **Cities** (victory condition).
   - Construct **Weapons**, **Horses**, **Boats**, or **Stockpiles**.
   - Fortify positions.

**Random Events** (plagues, strikes, bounties, etc.) can disrupt phases.

## Key Strategic Elements
- **Resource Management**: Stockpiles are vulnerable—if an enemy captures the territory holding your stockpile, they steal it.
- **Adjacency & Support**: Neighboring territories provide powerful flanking bonuses in combat.
- **Expansion vs. Development**: Balance conquering new land (for more resources) with building cities and military upgrades.
- **Computer AI**: Varies by personality; supports hot-seat multiplayer.
- **Victory**: First to meet the city goal (and defend it) wins. Ties or prolonged games continue until resolved.

The game rewards smart resource allocation, defensive positioning, and opportunistic attacks. Maps feel organic with irregular territories, water barriers, and strategic chokepoints.

This overview captures the essence of the original without modern adaptations. Use it as reference for implementing mechanics, phases, UI flow, or balancing in your clone.

## Resources in Detail
Resources are the heart of the game and drive both economy and military strength. They appear randomly on territories during map generation and are collected during the Production phase.

- **Gold**: The foundational resource. Used for general development, building cities, and basic upgrades. Territories with gold mines or similar features produce it reliably.
- **Horses**: Represent cavalry/mobility. Provide +1 force point per horse in combat. Essential for fast attacks and maintaining offensive pressure. Often found in pasture or open land territories.
- **Weapons**: Represent infantry or siege equipment. Grant a strong +3 force points each in combat. Critical for breaking heavily defended positions.
- **Boats**: Enable naval transport and sea-based attacks. Provide +2 force points and allow crossing water to reach non-adjacent (by land) territories. Vital on water-heavy maps.

**How Resources Work**:
- Resources are tied to specific territories. Controlling a resource-rich territory gives you ongoing production.
- You maintain a central **Stockpile** (one per player). Resources produced each turn flow into it.
- **Vulnerability**: The stockpile is located on one of your territories. If an enemy conquers that territory, they capture your entire stockpile — a major setback.
- **Development Spending**: In the Development phase, spend accumulated resources from your stockpile to:
  - Build Cities (victory buildings, also strong defensively).
  - Place military improvements (Horses, Weapons, Boats) on territories for permanent combat bonuses.
  - Create additional stockpiles or other structures.
- **Movement**: You can ship (move) your stockpile to safer or more strategic territories using boats or over land.
- **Trade**: In multiplayer games with 3+ players, you can negotiate direct resource trades.
- **Randomness & Balance**: Setup options control resource abundance. Beginner games often limit to just Gold + Horses; full games include all types for deeper strategy.

Controlling key resource territories while protecting your stockpile is often more important than raw territory count.

## What is the Stockpile?
The **Stockpile** is a central game mechanic representing each player's collected resources. Unlike resources that are tied to specific territories, the stockpile is a movable pool of **Gold, Horses, Weapons, Boats**, and other assets that you can spend or transport.

- Every player has **one primary stockpile**.
- It is physically located on **one of your territories** (you choose/move it strategically).
- All resources produced during the **Production** phase flow into your stockpile.

## Why the Stockpile Matters
- **Production Hub**: Territories generate resources each turn based on what they contain (e.g., gold mines, horse pastures). These resources automatically add to your stockpile.
- **Spending Power**: In the **Development** phase, you spend from the stockpile to:
  - Build **Cities** (the main victory condition).
  - Construct military improvements (**Horses**, **Weapons**, **Boats**) on your territories.
  - Create additional structures or fortifications.
- **Vulnerability**: Because the stockpile sits on a specific territory, it is a high-value target. If an enemy conquers that territory, they **capture your entire stockpile** — a devastating blow that hands them all your accumulated resources.

## Stockpile Movement & Logistics
- You can **ship/move** your stockpile during the **Shipment/Movement** phase.
  - Over land to adjacent territories you control.
  - Across water using **Boats**.
- Strategic placement is crucial:
  - Keep it on a well-defended central territory.
  - Move it away from threatened borders.
  - Position it near development sites for faster building.
- Multiple stockpiles? Some versions or advanced play allow secondary stockpiles, but the primary one is the main focus.

## Combat Interaction
- Stockpiles do **not** directly add to combat strength on their territory (unlike built Horses/Weapons).
- However, losing the stockpile territory not only loses resources but can cripple your future production and development for many turns.

## Tips for Stockpile Management
- **Protect it**: Fortify the hosting territory with Cities, adjacent support, and military units.
- **Balance Risk**: Early game, keep it safe while expanding. Late game, use it aggressively to rush cities.
- **Raiding**: Attacking an opponent's stockpile territory is often a game-changing move.
- **Trade**: In multiplayer, you can trade resources directly into/out of stockpiles.

Mastering the stockpile — producing efficiently, protecting it, and spending wisely — is often the difference between victory and defeat in *Lords of Conquest*.

This document can be used as a focused reference for implementing stockpile mechanics, UI (displaying contents, location, movement), vulnerability rules, and AI behavior in your clone.