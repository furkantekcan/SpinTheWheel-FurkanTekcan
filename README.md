# SpinTheWheel-FurkanTekcan

A Unity project integrating **Firebase Authentication**, **Firestore Database**, and a **Spin The Wheel reward system**.  
Implements asynchronous logic, cooldown mechanics, and external random API integration.

---

## 🚀 Features
- Firebase **Anonymous Authentication**
- Firestore user data model:
  - `spinCount`
  - `cooldownEndTimestamp`
  - `gold`
  - `gems`
  - `lastSpinTime`
- Async/await usage for smooth UI + network handling
- Spin animation with easing & cooldown timer
- Firestore integration with `[FirestoreData]` & `[FirestoreProperty]`
- Modular and clean architecture (SOLID-friendly)

---

## 📂 Setup Instructions
1. Clone the repository:  
   git clone https://github.com/yourusername/SpinTheWheel-[NameSurname].git
2. If Git LFS is not installed, install it:
```
git lfs install
```
3.To pull large files after cloning the repository:
```
git lfs pull
```
Otherwise, some necessary data files may be missing. 

### API Manipulation/Predictability

-**Risk**: The user may attempt to influence the outcome by bypassing the client and making requests directly to the randomness API.

-**Solution**: Randomness calculations should be performed server-side (e.g., Firebase Cloud Functions) whenever possible. API calls made through the client are vulnerable to manipulation.

### Cooldown Bypass

-**Risk**: The player can reset the cooldown by changing their device's clock.

-**Solution**: Cooldown information is stored in Firestore, and validation is performed based on server time rather than device time.
