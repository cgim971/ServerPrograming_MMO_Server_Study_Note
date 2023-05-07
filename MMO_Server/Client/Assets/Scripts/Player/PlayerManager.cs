using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager {
    private MyPlayer _myPlayer;
    // 플레이어들 목록
    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public static PlayerManager Instance { get; } = new PlayerManager();

    // 플레이어 리스트 생성 & 갱신 상황
    public void Add(S_PlayerList packet) {
        Object obj = Resources.Load("Player"); // Resources 산하의 Player Prefab load

        foreach (S_PlayerList.Player p in packet.players) {
            GameObject go = Object.Instantiate(obj) as GameObject; // 인게임 상에 클론 생성

            if (p.isSelf) { // Me
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.PlayerId = p.playerId;
                myPlayer.HealthSystem.Init(p.healthAmount);
                myPlayer.Life = p.Life;
                myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ); // 위치 세팅
                _myPlayer = myPlayer;
            }
            else { // Other
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.HealthSystem.Init(p.healthAmount);
                player.Life = p.Life;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ); // 위치 세팅
                _players.Add(p.playerId, player);
            }
        }
    }

    // 나 혹은 누군가가 새로 접속한 상황
    public void EnterGame(S_BroadcastEnterGame packet) {
        if (_myPlayer.PlayerId == packet.playerId) // 내가 들어옴 - 이중처리 방지
            return;

        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;

        Player player = go.AddComponent<Player>();
        player.PlayerId = packet.playerId;
        player.HealthSystem.Init(packet.healthAmount);
        player.Life = packet.Life;
        player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ); // 위치 세팅
        _players.Add(packet.playerId, player);
    }

    // 나 혹은 누군가가 게임을 떠난 상황
    public void LeaveGame(S_BroadcastLeaveGame packet) {
        if (_myPlayer.PlayerId == packet.playerId) { // 내가 나감
            GameObject.Destroy(_myPlayer.gameObject);
            _myPlayer = null;
        }
        else { // 다른이가 나감
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                GameObject.Destroy(player?.gameObject);
                _players.Remove(packet.playerId);
            }
        }
    }

    // 나 혹은 누군가가 움직임
    public void Move(S_BroadcastMove packet) {
        // 서버쪽에서 누군가가 이동한다는 OK pacekt이 왔을 때 그제서야 이동
        Vector3 currentPos = new Vector3(packet.posX, packet.posY, packet.posZ);
        Vector3 dir = new Vector3(packet.dirX, 0, packet.dirZ);
        if (_myPlayer.PlayerId == packet.playerId) { // 내가 이동
            //_myPlayer.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
            _myPlayer.Move(currentPos, dir);
        }
        else { // 다른이가 이동
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                //player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
                player.Move(currentPos, dir);
            }
        }
    }

    public void Rot(S_BroadcastRot packet) {
        if (_myPlayer.PlayerId == packet.playerId) { // 내가 이동
            _myPlayer.Rot(packet.angle);
        }
        else { // 다른이가 이동
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                player.Rot(packet.angle);
            }
        }
    }

    public void Fire(S_BroadcastFire packet) {
        if (_myPlayer.PlayerId == packet.playerId) { // 내가 이동
            _myPlayer.Gun.Fire(true);
        }
        else { // 다른이가 이동
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                player.Gun.Fire();
            }
        }
    }

    public void Hit(S_BroadcastHit packet) {
        if (_myPlayer.PlayerId == packet.hittedPlayerId) { // 내가 이동
            _myPlayer.HealthSystem.Damage(packet.playerHealth);
        }
        else { // 다른이가 이동
            Player player = null;
            if (_players.TryGetValue(packet.hittedPlayerId, out player)) {
                player.HealthSystem.Damage(packet.playerHealth);
            }
        }
    }

    public void Die(S_BroadcastDie packet) {
        if (_myPlayer.PlayerId == packet.playerId) { // 내가 이동
            _myPlayer.Life = packet.Life;
            _myPlayer.HealthSystem.Die();
        }
        else { // 다른이가 이동
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                player.Life = packet.Life;
                player.HealthSystem.Die();
            }
        }
    }

    public void Revive(S_BroadcastRevive packet) {
        if (_myPlayer.PlayerId == packet.playerId) { // 내가 이동
            _myPlayer.HealthSystem.Revive(packet.healthAmount);
        }
        else { // 다른이가 이동
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                player.HealthSystem.Revive(packet.healthAmount);
            }
        }
    }
}