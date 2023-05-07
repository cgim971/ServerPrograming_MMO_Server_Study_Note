using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager {
    private MyPlayer _myPlayer;
    // �÷��̾�� ���
    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public static PlayerManager Instance { get; } = new PlayerManager();

    // �÷��̾� ����Ʈ ���� & ���� ��Ȳ
    public void Add(S_PlayerList packet) {
        Object obj = Resources.Load("Player"); // Resources ������ Player Prefab load

        foreach (S_PlayerList.Player p in packet.players) {
            GameObject go = Object.Instantiate(obj) as GameObject; // �ΰ��� �� Ŭ�� ����

            if (p.isSelf) { // Me
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.PlayerId = p.playerId;
                myPlayer.HealthSystem.Init(p.healthAmount);
                myPlayer.Life = p.Life;
                myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ); // ��ġ ����
                _myPlayer = myPlayer;
            }
            else { // Other
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.HealthSystem.Init(p.healthAmount);
                player.Life = p.Life;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ); // ��ġ ����
                _players.Add(p.playerId, player);
            }
        }
    }

    // �� Ȥ�� �������� ���� ������ ��Ȳ
    public void EnterGame(S_BroadcastEnterGame packet) {
        if (_myPlayer.PlayerId == packet.playerId) // ���� ���� - ����ó�� ����
            return;

        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;

        Player player = go.AddComponent<Player>();
        player.PlayerId = packet.playerId;
        player.HealthSystem.Init(packet.healthAmount);
        player.Life = packet.Life;
        player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ); // ��ġ ����
        _players.Add(packet.playerId, player);
    }

    // �� Ȥ�� �������� ������ ���� ��Ȳ
    public void LeaveGame(S_BroadcastLeaveGame packet) {
        if (_myPlayer.PlayerId == packet.playerId) { // ���� ����
            GameObject.Destroy(_myPlayer.gameObject);
            _myPlayer = null;
        }
        else { // �ٸ��̰� ����
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                GameObject.Destroy(player?.gameObject);
                _players.Remove(packet.playerId);
            }
        }
    }

    // �� Ȥ�� �������� ������
    public void Move(S_BroadcastMove packet) {
        // �����ʿ��� �������� �̵��Ѵٴ� OK pacekt�� ���� �� �������� �̵�
        Vector3 currentPos = new Vector3(packet.posX, packet.posY, packet.posZ);
        Vector3 dir = new Vector3(packet.dirX, 0, packet.dirZ);
        if (_myPlayer.PlayerId == packet.playerId) { // ���� �̵�
            //_myPlayer.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
            _myPlayer.Move(currentPos, dir);
        }
        else { // �ٸ��̰� �̵�
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                //player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
                player.Move(currentPos, dir);
            }
        }
    }

    public void Rot(S_BroadcastRot packet) {
        if (_myPlayer.PlayerId == packet.playerId) { // ���� �̵�
            _myPlayer.Rot(packet.angle);
        }
        else { // �ٸ��̰� �̵�
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                player.Rot(packet.angle);
            }
        }
    }

    public void Fire(S_BroadcastFire packet) {
        if (_myPlayer.PlayerId == packet.playerId) { // ���� �̵�
            _myPlayer.Gun.Fire(true);
        }
        else { // �ٸ��̰� �̵�
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                player.Gun.Fire();
            }
        }
    }

    public void Hit(S_BroadcastHit packet) {
        if (_myPlayer.PlayerId == packet.hittedPlayerId) { // ���� �̵�
            _myPlayer.HealthSystem.Damage(packet.playerHealth);
        }
        else { // �ٸ��̰� �̵�
            Player player = null;
            if (_players.TryGetValue(packet.hittedPlayerId, out player)) {
                player.HealthSystem.Damage(packet.playerHealth);
            }
        }
    }

    public void Die(S_BroadcastDie packet) {
        if (_myPlayer.PlayerId == packet.playerId) { // ���� �̵�
            _myPlayer.Life = packet.Life;
            _myPlayer.HealthSystem.Die();
        }
        else { // �ٸ��̰� �̵�
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                player.Life = packet.Life;
                player.HealthSystem.Die();
            }
        }
    }

    public void Revive(S_BroadcastRevive packet) {
        if (_myPlayer.PlayerId == packet.playerId) { // ���� �̵�
            _myPlayer.HealthSystem.Revive(packet.healthAmount);
        }
        else { // �ٸ��̰� �̵�
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player)) {
                player.HealthSystem.Revive(packet.healthAmount);
            }
        }
    }
}