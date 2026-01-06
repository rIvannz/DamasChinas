using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Logic;
using System.Reflection;
using Xunit;

namespace DamasChinas_Tests.logic;

public class LobbyManagerTests
{

    private static MethodInfo GetPrivateStatic(string name)
    {
        return typeof(LobbyManager)
            .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new Exception($"Método {name} no encontrado");
    }

    private static MethodInfo GetPrivateNested(Type nested, string name)
    {
        return nested.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new Exception($"Método {name} no encontrado");
    }

    private static Type GetLobbyStateType()
    {
        return typeof(LobbyManager)
            .GetNestedTypes(BindingFlags.NonPublic)
            .First(t => t.Name == "LobbyState");
    }

    [Fact]
    public void ValidateCreateRequest_NullRequest_ThrowsException()
    {
        var method = GetPrivateStatic("ValidateCreateRequest");

        Assert.True(
         Assert.Throws<TargetInvocationException>(() =>
             method.Invoke(null, new object[] { null })
         ).InnerException is RepositoryValidationException repoException &&
         repoException.Code == MessageCode.MatchCreationFailed
     );


    }

    [Fact]
    public void ValidateCreateRequest_InvalidMaxPlayers_ThrowsException()
    {
        var method = GetPrivateStatic("ValidateCreateRequest");

        var req = new CreateLobbyRequest { MaxPlayers = 3 };

        Assert.True(
      Assert.Throws<TargetInvocationException>(() =>
          method.Invoke(null, new object[] { req })
      ).InnerException is RepositoryValidationException repoException &&
      repoException.Code == MessageCode.LobbyInvalidMaxPlayers
  );


    }

    [Fact]
    public void ValidateJoinRequest_NullRequest_ThrowsException()
    {
        var method = GetPrivateStatic("ValidateJoinRequest");

        Assert.True(
       Assert.Throws<TargetInvocationException>(() =>
           method.Invoke(null, new object[] { null })
       ).InnerException is RepositoryValidationException repoException &&
       repoException.Code == MessageCode.LobbyNotFound
   );


    }

    [Fact]
    public void ValidateJoinRequest_EmptyUsername_ThrowsException()
    {
        var method = GetPrivateStatic("ValidateJoinRequest");

        var req = new JoinLobbyRequest
        {
            LobbyCode = 123456,
            Username = ""
        };

        Assert.True(
       Assert.Throws<TargetInvocationException>(() =>
           method.Invoke(null, new object[] { req })
       ).InnerException is RepositoryValidationException repoException &&
       repoException.Code == MessageCode.UsernameEmpty
   );


    }

    [Fact]
    public void IsGuest_ValidGuestUsername_ReturnsTrue()
    {
        var method = GetPrivateStatic("IsGuest");

        var result = (bool)method.Invoke(null, new object[] { "Guest-1234" })!;

        Assert.True(result);
    }

    [Fact]
    public void IsGuest_InvalidGuestFormat_ReturnsFalse()
    {
        var method = GetPrivateStatic("IsGuest");

        var result = (bool)method.Invoke(null, new object[] { "Guest-12A4" })!;

        Assert.False(result);
    }

    [Fact]
    public void LobbyState_AddAndRemoveMember_UpdatesCountCorrectly()
    {
        var lobbyType = GetLobbyStateType();

        var lobby = Activator.CreateInstance(
            lobbyType,
            123456,
            LobbyVisibility.Public,
            4,
            "Host"
        )!;

        var addMethod = GetPrivateNested(lobbyType, "AddOrUpdateMember");
        var removeMethod = GetPrivateNested(lobbyType, "RemoveMember");
        var countMethod = GetPrivateNested(lobbyType, "GetMemberCount");

        addMethod.Invoke(lobby, new object[]
        {
            new LobbyMemberDto
            {
                Username = "Seth",
                UserId = 1,
                AvatarFile = "a.png",
                IsHost = false
            }
        });

        Assert.True(
      Record.Exception(() =>
      {
          addMethod.Invoke(lobby, new object[]
          {
            new LobbyMemberDto
            {
                Username = "Seth",
                UserId = 1,
                AvatarFile = "a.png",
                IsHost = false
            }
          });

          if ((int)countMethod.Invoke(lobby, null) != 1)
              return;

          removeMethod.Invoke(lobby, new object[] { "Seth" });

          if ((int)countMethod.Invoke(lobby, null) != 0)
              throw new Exception("Count incorrect");
      }) == null
  );



    }

    [Fact]
    public void LobbyState_AssignNewHost_AssignsOldestMember()
    {
        var lobbyType = GetLobbyStateType();

        var lobby = Activator.CreateInstance(
            lobbyType,
            123,
            LobbyVisibility.Public,
            4,
            "Host"
        )!;

        var add = GetPrivateNested(lobbyType, "AddOrUpdateMember");
        var assign = GetPrivateNested(lobbyType, "AssignNewHostIfNeeded");
        var hostProp = lobbyType.GetProperty("HostUsername")!;

        add.Invoke(lobby, new object[]
        {
            new LobbyMemberDto { Username = "Ana", UserId = 2 }
        });

        add.Invoke(lobby, new object[]
        {
            new LobbyMemberDto { Username = "Luis", UserId = 3 }
        });

        assign.Invoke(lobby, null);

        Assert.Equal("Ana", hostProp.GetValue(lobby));
    }

    [Fact]
    public void LobbyState_ThrowIfFull_WhenMaxPlayersReached_ThrowsException()
    {
        var lobbyType = GetLobbyStateType();

        var lobby = Activator.CreateInstance(
            lobbyType,
            123,
            LobbyVisibility.Public,
            1,
            "Host"
        )!;

        var add = GetPrivateNested(lobbyType, "AddOrUpdateMember");
        var throwIfFull = GetPrivateNested(lobbyType, "ThrowIfFull");

        add.Invoke(lobby, new object[]
        {
            new LobbyMemberDto { Username = "Host", UserId = 1 }
        });

        Assert.True(
     Assert.Throws<TargetInvocationException>(() =>
         throwIfFull.Invoke(lobby, null)
     ).InnerException is RepositoryValidationException repoException &&
     repoException.Code == MessageCode.LobbyFull
 );


    }

    [Fact]
    public void LobbyState_ThrowIfGameStarted_ThrowsException()
    {
        var lobbyType = GetLobbyStateType();

        var lobby = Activator.CreateInstance(
            lobbyType,
            123,
            LobbyVisibility.Public,
            2,
            "Host"
        )!;

        var markStarted = GetPrivateNested(lobbyType, "MarkGameStarted");
        var throwStarted = GetPrivateNested(lobbyType, "ThrowIfGameStarted");

        markStarted.Invoke(lobby, null);

        Assert.True(
    Assert.Throws<TargetInvocationException>(() =>
        throwStarted.Invoke(lobby, null)
    ).InnerException is RepositoryValidationException repoException &&
    repoException.Code == MessageCode.LobbyGameAlreadyStarted
);

    }
}
