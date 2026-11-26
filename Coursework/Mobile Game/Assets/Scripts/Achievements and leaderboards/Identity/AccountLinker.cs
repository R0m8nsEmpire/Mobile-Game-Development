// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// AccountLinker: attach a platform account to the Unity player ("Linking a platform account" slide).
//
// Compiles only with UGS_AUTH_INSTALLED (Authentication package, com.unity.services.authentication) and does something
// only when a platform adapter is also compiled in (GPGS_INSTALLED or APPLE_GAMEKIT_INSTALLED).
//
// The rule from the identity section: the Unity player id is the anchor. Sign in anonymously first (Week 7 does this),
// then LINK the platform account to that player. If the platform account already belongs to a different Unity player,
// Unity Authentication throws AuthenticationException with ErrorCode AccountAlreadyLinked: ask the player, and if they
// choose the other account, SIGN IN with the platform credential instead (which switches player, so reload the save).
//
// From the Unity Authentication documentation:
//   Google Play Games: RequestServerSideAccess(true, code) on the plug-in gives a one-time auth code;
//     LinkWithGooglePlayGamesAsync(authCode) / SignInWithGooglePlayGamesAsync(authCode); UnlinkGooglePlayGamesAsync().
//     Dashboard: Authentication > ID Providers > Google Play Games with the web client ID and secret. Plug-in v11.01+.
//   Apple Game Center: GKLocalPlayer.Local.FetchItems() gives signature, salt, public key URL and timestamp;
//     LinkWithAppleGameCenterAsync(signature, teamPlayerId, publicKeyURL, salt, timestamp) / SignInWith...;
//     UnlinkAppleGameCenterAsync(). The items are valid for ten minutes. Dashboard: ID Providers > Apple Game Center
//     with the bundle ID. Authentication SDK 2.4.0 or later.

#if UGS_AUTH_INSTALLED
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

namespace CMP5402.Week9
{
    public static class AccountLinker
    {
        public enum Outcome { Linked, AlreadyLinkedElsewhere, NotAvailable, Failed }

        /// Link the signed-in platform account of the hub's back end to the current Unity player.
        public static async Task<Outcome> LinkCurrentPlatformAsync(PlatformHub hub)
        {
            if (hub == null || !hub.IsSignedIn) return Outcome.NotAvailable;
#if GPGS_INSTALLED
            if (hub.Services is PlayGamesServices pg)
            {
                var code = await RequestCodeAsync(pg);
                if (string.IsNullOrEmpty(code)) return Outcome.Failed;
                return await LinkOrReportAsync(() => AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(code));
            }
#endif
#if APPLE_GAMEKIT_INSTALLED
            if (hub.Services is GameCenterServices gc)
            {
                var items = await gc.FetchIdentityAsync();   // fresh: valid for ten minutes
                if (items == null) return Outcome.Failed;
                return await LinkOrReportAsync(() => AuthenticationService.Instance.LinkWithAppleGameCenterAsync(
                    Convert.ToBase64String(items.GetSignature()),
                    Apple.GameKit.GKLocalPlayer.Local.TeamPlayerId,
                    items.PublicKeyUrl,
                    Convert.ToBase64String(items.GetSalt()),
                    items.Timestamp));
            }
#endif
            return Outcome.NotAvailable;
        }

        /// The player chose the OTHER Unity account that this platform account already owns. Switches player:
        /// the caller must reload the save for the new player id afterwards.
        public static async Task<bool> SignInWithCurrentPlatformAsync(PlatformHub hub)
        {
            if (hub == null || !hub.IsSignedIn) return false;
            try
            {
#if GPGS_INSTALLED
                if (hub.Services is PlayGamesServices pg)
                {
                    var code = await RequestCodeAsync(pg);
                    if (string.IsNullOrEmpty(code)) return false;
                    if (AuthenticationService.Instance.IsSignedIn) AuthenticationService.Instance.SignOut();
                    await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(code);
                    return true;
                }
#endif
#if APPLE_GAMEKIT_INSTALLED
                if (hub.Services is GameCenterServices gc)
                {
                    var items = await gc.FetchIdentityAsync();
                    if (items == null) return false;
                    if (AuthenticationService.Instance.IsSignedIn) AuthenticationService.Instance.SignOut();
                    await AuthenticationService.Instance.SignInWithAppleGameCenterAsync(
                        Convert.ToBase64String(items.GetSignature()), Apple.GameKit.GKLocalPlayer.Local.TeamPlayerId,
                        items.PublicKeyUrl, Convert.ToBase64String(items.GetSalt()), items.Timestamp);
                    return true;
                }
#endif
            }
            catch (Exception ex) { Debug.LogWarning("[Auth] platform sign-in failed: " + ex.Message); }
            return false;
        }

        static async Task<Outcome> LinkOrReportAsync(Func<Task> link)
        {
            try
            {
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();   // the player id must exist first
                await link();
                return Outcome.Linked;
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                return Outcome.AlreadyLinkedElsewhere;   // ask the player; never merge silently
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Auth] link failed: " + ex.Message);
                return Outcome.Failed;
            }
        }

#if GPGS_INSTALLED
        static Task<string> RequestCodeAsync(PlayGamesServices pg)
        {
            var tcs = new TaskCompletionSource<string>();
            pg.RequestAuthCode(code => tcs.TrySetResult(code));
            return tcs.Task;
        }
#endif
    }
}
#endif
