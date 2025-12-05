# Challenge Mode - Schedule I Mod

A difficulty mod for Schedule I that makes customers significantly more selective about their purchases based on drug type preferences and product properties.

## Features

### 1. Enhanced Product Enjoyment System
Customers now have much stronger preferences based on:
- **Drug Type Affinity**: Up to ±30% enjoyment modifier based on how much they like/dislike specific drug types (Marijuana, Cocaine, Meth, etc.)
- **Property Matching**: Up to ±20% enjoyment modifier based on how well product effects match their preferred properties
- Replaces the vanilla system with more impactful preference weighting

### 2. Stricter Direct Sales (Handover)
Success rates for direct sales are drastically reduced when offering mismatched products:
- **<30% enjoyment**: 90% success rate reduction (0.1x multiplier)
- **30-50% enjoyment**: 70% success rate reduction (0.3x multiplier)
- **50-70% enjoyment**: 40% success rate reduction (0.6x multiplier)
- **>70% enjoyment**: No penalty (1.0x multiplier)

### 3. Tougher Counter-Offers
When customers request specific products via messages, counter-offering is now much harder:
- **Price limit reduced**: From 3x to 2x average spend
- **Minimum enjoyment requirement**: 40% enjoyment threshold (rejects below)
- **Random rejection chance**: 50% chance to reject counter-offers with 40-60% enjoyment

### 4. Drug Affinity Display
Customer handover screens now show their drug type preferences:
- Color-coded affinity indicators:
  - **Green [+++]**: High affinity (>0.6)
  - **Light Green [++]**: Medium-high affinity (0.2-0.6)
  - **Yellow [+]**: Neutral (-0.2 to 0.2)
  - **Orange [-]**: Low affinity (-0.6 to -0.2)
  - **Red [--]**: Strong dislike (<-0.6)

## Installation

### Requirements
- Schedule I (MONO version)
- MelonLoader installed in game directory
- .NET SDK 6.0 or later (for building from source)

### Install Pre-built
1. Download `challange_mode.dll` from releases
2. Place in `[Game Directory]/Mods/`
3. Launch the game

### Build from Source
1. Clone this repository
2. Open terminal in `MONO_IL2CPP_Template` directory
3. Run `dotnet build -c MONO`
4. DLL will be compiled to `bin/MONO/netstandard2.1/challange_mode.dll`
5. Copy to your game's `Mods` folder

## Configuration

Game path is configured in the `.csproj` file:
```xml
<S1Dir>E:\SteamLibrary\steamapps\common\Schedule I</S1Dir>
```

Update this path if your game is installed elsewhere.

## Gameplay Impact

This mod significantly increases difficulty by:
- Making it nearly impossible to sell products customers don't want
- Requiring you to stock diverse drug types to satisfy different customers
- Forcing strategic product matching based on customer preferences
- Making counter-offers much harder to negotiate

**Recommendation**: Check customer drug affinities in the handover screen before stocking products!

## Technical Details

### Patched Methods
- `Customer.GetProductEnjoyment()` - Modified enjoyment calculation
- `Customer.GetOfferSuccessChance()` - Added enjoyment-based multipliers
- `Customer.EvaluateCounteroffer()` - Stricter acceptance criteria
- `HandoverScreenDetailPanel.Open()` - Added drug affinity UI

### Dependencies
- HarmonyLib (for runtime patching)
- MelonLoader (mod loader)
- Schedule I game assemblies

## Compatibility

- **Game Version**: Tested on current Schedule I MONO build
- **Other Mods**: Should be compatible with most mods unless they also modify customer behavior
- **IL2CPP**: Not currently supported (MONO only)

## Known Issues

None reported. Please submit issues on the GitHub repository.

## Future Features (Planned)

### 1. Customer Feedback Messages
Add messaging system where customers periodically send messages when they can't find products they like in your current offerings.

### 2. Enhanced Addiction & Satisfaction Scaling
Rework the addiction and satisfaction systems so customers offer increasingly higher prices and product amounts as their addiction grows, with much steeper scaling than vanilla.

### 3. Dynamic Pricing System
Replace manual price configuration with customer-driven pricing based on deal success rates and demand. Prices adjust automatically based on market forces.

### 4. Increased Penalties
Implement harsher consequences for:
- Rejected deals (larger relationship penalties)
- Customers unable to find desired products
- Failed contract acceptances
- Unmet customer needs

### 5. Dynamic Customer Preferences
Make customer affinities and preferences change over time to prevent the game from becoming stale. Customers will gradually shift their drug type preferences and desired effects, requiring players to adapt their product offerings continuously.

## Credits

Created for enhanced Schedule I gameplay challenge.

## License

This project uses the Schedule I game assemblies for modding purposes only.
