using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using System;
using System.Collections.Generic;
using System.Threading;
using SocialFriendRequest = DCL.Social.Friends.FriendRequest;

namespace DCL.Social.Friends
{
    public interface IFriendsController : IService
    {
        event Action OnInitialized;
        event Action<string, FriendshipAction> OnUpdateFriendship;
        event Action<string, UserStatus> OnUpdateUserStatus;
        event Action<string> OnFriendNotFound;
        event Action<List<FriendWithDirectMessages>> OnAddFriendsWithDirectMessages;
        event Action<int, int> OnTotalFriendRequestUpdated;
        event Action<SocialFriendRequest> OnFriendRequestReceived;
        event Action<SocialFriendRequest> OnSentFriendRequestApproved;

        int AllocatedFriendCount { get; }
        bool IsInitialized { get; }
        int ReceivedRequestCount { get; }
        int TotalFriendCount { get; }
        int TotalFriendRequestCount { get; }
        int TotalReceivedFriendRequestCount { get; }
        int TotalSentFriendRequestCount { get; }
        int TotalFriendsWithDirectMessagesCount { get; }

        Dictionary<string, UserStatus> GetAllocatedFriends();

        UserStatus GetUserStatus(string userId);

        bool ContainsStatus(string friendId, FriendshipStatus status);

        UniTask<SocialFriendRequest> RequestFriendshipAsync(string friendUserId, string messageBody, CancellationToken cancellationToken = default);

        UniTask<SocialFriendRequest> CancelRequestByUserIdAsync(string friendUserId, CancellationToken cancellationToken = default);

        UniTask<SocialFriendRequest> CancelRequestAsync(string friendRequestId, CancellationToken cancellationToken = default);

        UniTask<SocialFriendRequest> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default);


        UniTask<SocialFriendRequest> RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default);

        [Obsolete("Deprecated and will be removed in the future. Due lazy loading it may raise state inconsistencies. Use GetFriendshipStatus instead")]
        bool IsFriend(string userId);

        UniTask RemoveFriendAsync(string friendId, CancellationToken cancellationToken);

        UniTask<string[]> GetFriendsAsync(int limit, int skip, CancellationToken cancellationToken = default);

        UniTask<IReadOnlyList<string>> GetFriendsAsync(string usernameOrId, int limit, CancellationToken cancellationToken = default);

        UniTask<IReadOnlyList<SocialFriendRequest>> GetFriendRequestsAsync(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip, CancellationToken cancellationToken = default);

        void GetFriendsWithDirectMessages(int limit, int skip);

        void GetFriendsWithDirectMessages(string userNameOrId, int limit);

        public bool TryGetAllocatedFriendRequest(string friendRequestId, out FriendRequest result);

        SocialFriendRequest GetAllocatedFriendRequestByUser(string userId);

        UniTask<FriendshipStatus> GetFriendshipStatus(string userId, CancellationToken cancellationToken = default);
    }
}
