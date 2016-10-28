﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Facepunch.Steamworks.Interop
{
    internal class NativeInterface : IDisposable
    {
        internal SteamNative.SteamApi api;
        internal SteamNative.SteamClient client;
        internal SteamNative.SteamUser user;
        internal SteamNative.SteamApps apps;
        internal SteamNative.SteamFriends friends;
        internal SteamNative.SteamMatchmakingServers servers;
        internal SteamNative.SteamInventory inventory;
        internal SteamNative.SteamNetworking networking;
        internal SteamNative.SteamUserStats userstats;
        internal SteamNative.SteamUtils utils;
        internal SteamNative.SteamScreenshots screenshots;
        internal SteamNative.SteamHTTP http;
        internal SteamNative.SteamUGC ugc;
        internal SteamNative.SteamGameServer gameServer;
        internal SteamNative.SteamGameServerStats gameServerStats;
        internal SteamNative.SteamRemoteStorage remoteStorage;

        private bool isServer;

        internal bool InitClient()
        {
            isServer = false;

            api = new SteamNative.SteamApi( (IntPtr) 1 );

            if ( !api.SteamAPI_Init() )
                return false;

            var huser = api.SteamAPI_GetHSteamUser();
            var hpipe = api.SteamAPI_GetHSteamPipe();
            if ( hpipe == 0 )
                return false;

            FillInterfaces( huser, hpipe );

            // Ensure that the user has logged into Steam. This will always return true if the game is launched
            // from Steam, but if Steam is at the login prompt when you run your game it will return false.
            if ( !user.BLoggedOn() )
                return false;

            return true;
        }

        internal bool InitServer( uint IpAddress /*uint32*/, ushort usPort /*uint16*/, ushort GamePort /*uint16*/, ushort QueryPort /*uint16*/, int eServerMode /*int*/, string pchVersionString /*const char **/)
        {
            isServer = true;

            api = new SteamNative.SteamApi( (IntPtr)1 );

            if ( !api.SteamInternal_GameServer_Init( IpAddress, usPort, GamePort, QueryPort, eServerMode, pchVersionString ) )
            {
                return false;
            }

            var user = api.SteamGameServer_GetHSteamUser();
            var pipe = api.SteamGameServer_GetHSteamPipe();
            if ( pipe == 0 )
                return false;

            FillInterfaces( pipe, user );

            if ( !gameServer.IsValid )
            {
                gameServer = null;
                throw new System.Exception( "Steam Server: Couldn't load SteamGameServer012" );
            }

            return true;
        }

        public void FillInterfaces( int hpipe, int huser )
        {
            var clientPtr = api.SteamInternal_CreateInterface( "SteamClient017" );
            if ( clientPtr == IntPtr.Zero )
            {
                throw new System.Exception( "Steam Server: Couldn't load SteamClient017" );
            }

            client = new SteamNative.SteamClient( clientPtr );

            user = client.GetISteamUser( huser, hpipe, "SteamUser019" );
            utils = client.GetISteamUtils( hpipe, "SteamUtils008" );
            networking = client.GetISteamNetworking( huser, hpipe, "SteamNetworking005" );
            gameServerStats = client.GetISteamGameServerStats( huser, hpipe, "SteamGameServerStats001" );
            http = client.GetISteamHTTP( huser, hpipe, "STEAMHTTP_INTERFACE_VERSION002" );
            inventory = client.GetISteamInventory( huser, hpipe, "STEAMINVENTORY_INTERFACE_V001" );
            ugc = client.GetISteamUGC( huser, hpipe, "STEAMUGC_INTERFACE_VERSION009" );
            apps = client.GetISteamApps( huser, hpipe, "STEAMAPPS_INTERFACE_VERSION008" );
            gameServer = client.GetISteamGameServer( huser, hpipe, "SteamGameServer012" );
            friends = client.GetISteamFriends( huser, hpipe, "SteamFriends015" );
            servers = client.GetISteamMatchmakingServers( huser, hpipe, "SteamMatchMakingServers002" );
            userstats = client.GetISteamUserStats( huser, hpipe, "STEAMUSERSTATS_INTERFACE_VERSION011" );
            screenshots = client.GetISteamScreenshots( huser, hpipe, "STEAMSCREENSHOTS_INTERFACE_VERSION003" );
            remoteStorage = client.GetISteamRemoteStorage( huser, hpipe, "STEAMREMOTESTORAGE_INTERFACE_VERSION014" );
        }

        public void Dispose()
        {
            if ( client != null )
            {
                client.Dispose();
                client = null;
            }

            if ( user != null )
            {
                user.Dispose();
                user = null;
            }

            if ( utils != null )
            {
                utils.Dispose();
                utils = null;
            }

            if ( networking != null )
            {
                networking.Dispose();
                networking = null;
            }

            if ( gameServerStats != null )
            {
                gameServerStats.Dispose();
                gameServerStats = null;
            }

            if ( http != null )
            {
                http.Dispose();
                http = null;
            }

            if ( inventory != null )
            {
                inventory.Dispose();
                inventory = null;
            }

            if ( ugc != null )
            {
                ugc.Dispose();
                ugc = null;
            }

            if ( apps != null )
            {
                apps.Dispose();
                apps = null;
            }

            if ( gameServer != null )
            {
                //
                // Calling this can cause the process to hang
                //
                //gameServer.LogOff();
                
                gameServer.Dispose();
                gameServer = null;
            }

            if ( friends != null )
            {
                friends.Dispose();
                friends = null;
            }

            if ( servers != null )
            {
                servers.Dispose();
                servers = null;
            }

            if ( userstats != null )
            {
                userstats.Dispose();
                userstats = null;
            }

            if ( screenshots != null )
            {
                screenshots.Dispose();
                screenshots = null;
            }

            if ( remoteStorage != null )
            {
                remoteStorage.Dispose();
                remoteStorage = null;
            }

            if ( api != null )
            {
                if ( isServer )
                    api.SteamGameServer_Shutdown();
                else
                    api.SteamAPI_Shutdown();

                api.Dispose();
                api = null;
            }
        }
    }
}
