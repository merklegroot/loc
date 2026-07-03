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