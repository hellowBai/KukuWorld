using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 捕捉系统 - 管理捕捉逻辑
    /// </summary>
    public class CaptureSystem : MonoBehaviour
    {
        // 野外神宠结构
        public struct WildPet
        {
            public MythicalPetData petData;
            public Vector3 position;
            public float remainingHP;
            public bool isCapturable;
            public float timeUntilDespawn;                        // 消失倒计时
            public bool isSoulAbsorbing;                         // 是否正在吸收灵魂
            public float soulAccumulated;                        // 已积累的灵魂值

            public WildPet(MythicalPetData pet, Vector3 pos)
            {
                petData = pet;
                position = pos;
                remainingHP = pet.Health;
                isCapturable = false; // 初始不可捕捉，需要削弱后才能捕捉
                timeUntilDespawn = 300f; // 5分钟内必须捕捉，否则消失
                isSoulAbsorbing = false;
                soulAccumulated = 0f;
            }
        }

        // 捕捉结果结构
        public struct CaptureResult
        {
            public bool success;
            public MythicalPetData capturedPet;
            public string message;
            public List<ItemDrop> drops;                         // 捕捉成功后的掉落物

            public CaptureResult(bool isSuccess, MythicalPetData pet, string msg, List<ItemDrop> dropList = null)
            {
                success = isSuccess;
                capturedPet = pet;
                message = msg;
                drops = dropList ?? new List<ItemDrop>();
            }
        }

        // 掉落物结构
        public struct ItemDrop
        {
            public ItemType type;
            public float value;
            public int quantity;
            
            public enum ItemType { Soul, Coin, PetEgg, Resource }
            
            public ItemDrop(ItemType t, float val, int qty)
            {
                type = t;
                value = val;
                quantity = qty;
            }
        }

        // 配置参数
        [Header("探索设置")]
        public float explorationAreaSize = 20f;                    // 探索区域大小
        public int maxWildPets = 10;                              // 最大野外神宠数量
        public float wildPetRespawnTime = 30f;                    // 重生时间
        public float explorationCheckInterval = 5f;               // 探索检查间隔
        public float capturePhaseDuration = 300f;                 // 捕捉阶段持续时间(5分钟)

        // 探捉阶段控制
        private float timeInCapturePhase = 0f;                    // 捕捉阶段已进行时间
        private bool capturePhaseActive = false;                  // 捕捉阶段是否激活
        private List<WildPet> wildPets = new List<WildPet>();     // 野外神宠列表

        // 事件
        public System.Action<Vector3> OnWildPetSpotted;            // 发现神宠事件
        public System.Action<string> OnExplorationEvent;           // 探索事件
        public System.Action OnCapturePhaseEnded;                  // 捕捉阶段结束事件
        public System.Action OnDefensePhaseStarted;                // 防守阶段开始事件

        void Start()
        {
            InitializeExplorationSystem();
        }

        void Update()
        {
            if (capturePhaseActive)
            {
                // 更新捕捉阶段时间
                timeInCapturePhase += Time.deltaTime;
                
                // 检查是否超过捕捉阶段时间
                if (timeInCapturePhase >= capturePhaseDuration)
                {
                    EndCapturePhase();
                }
                
                // 定期检查是否需要生成新的野生神宠
                CheckAndRespawnWildPets();
            }
        }

        /// <summary>
        /// 初始化探索系统
        /// </summary>
        private void InitializeExplorationSystem()
        {
            capturePhaseActive = false;
            timeInCapturePhase = 0f;
            wildPets.Clear();
            
            Debug.Log("捕捉系统初始化完成");
        }

        /// <summary>
        /// 开始捕捉阶段
        /// </summary>
        public void StartCapturePhase()
        {
            capturePhaseActive = true;
            timeInCapturePhase = 0f;
            wildPets.Clear();
            
            Debug.Log("捕捉阶段开始，快去捕捉野生神宠！");
        }

        /// <summary>
        /// 结束捕捉阶段
        /// </summary>
        public void EndCapturePhase()
        {
            capturePhaseActive = false;
            OnCapturePhaseEnded?.Invoke();
            
            Debug.Log("捕捉阶段结束！");
        }

        /// <summary>
        /// 检查并重生野生神宠
        /// </summary>
        private void CheckAndRespawnWildPets()
        {
            // 检查当前野外神宠数量并生成新的
            if (wildPets.Count < maxWildPets)
            {
                // 根据捕捉阶段的时间推移，逐渐提高生成的宠物稀有度
                float timeRatio = timeInCapturePhase / capturePhaseDuration;
                
                // 根据时间比例调整稀有度
                MythicalPetData.MythicalRarity rarity = GetRarityBasedOnTime(timeRatio);
                
                // 随机生成位置
                Vector3 randomPos = GetRandomExplorationPosition();
                
                // 检查该位置是否已经有神宠
                if (!HasWildPetAtPosition(randomPos, 2f))
                {
                    GenerateWildPet(randomPos, rarity);
                    
                    // 通知发现野生神宠
                    OnWildPetSpotted?.Invoke(randomPos);
                    
                    Debug.Log($"在位置 {randomPos} 生成了野生神宠");
                }
            }
        }

        /// <summary>
        /// 根据时间比例获取稀有度
        /// </summary>
        private MythicalPetData.MythicalRarity GetRarityBasedOnTime(float timeRatio)
        {
            if (timeRatio < 0.2f) // 前20%时间
                return MythicalPetData.MythicalRarity.Celestial;
            else if (timeRatio < 0.4f) // 20%-40%时间
                return MythicalPetData.MythicalRarity.Immortal;
            else if (timeRatio < 0.6f) // 40%-60%时间
                return MythicalPetData.MythicalRarity.DivineBeast;
            else if (timeRatio < 0.8f) // 60%-80%时间
                return MythicalPetData.MythicalRarity.Sacred;
            else // 最后20%时间
                return MythicalPetData.MythicalRarity.Primordial;
        }

        /// <summary>
        /// 获取随机探索位置
        /// </summary>
        private Vector3 GetRandomExplorationPosition()
        {
            float x = Random.Range(-explorationAreaSize / 2, explorationAreaSize / 2);
            float z = Random.Range(-explorationAreaSize / 2, explorationAreaSize / 2);
            
            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// 生成野外神宠
        /// </summary>
        public void GenerateWildPet(Vector3 position, MythicalPetData.MythicalRarity rarity = MythicalPetData.MythicalRarity.Celestial)
        {
            try
            {
                // 创建野生神宠数据
                MythicalPetData wildPet = new MythicalPetData();
                wildPet.Id = Random.Range(1000, 9999); // 随机ID
                wildPet.Name = GenerateWildPetName(rarity);
                wildPet.Rarity = rarity;
                wildPet.Description = "在野外游荡的神宠";
                wildPet.SpriteName = $"WildPet_{wildPet.Id}";
                
                // 设置基础属性
                wildPet.AttackPower = 30f + (int)rarity * 20f;
                wildPet.DefensePower = 20f + (int)rarity * 15f;
                wildPet.Speed = 2f + (int)rarity * 0.5f;
                wildPet.Health = 100f + (int)rarity * 50f;
                
                // 设置神话属性
                wildPet.DivinePower = 20f + (int)rarity * 15f;
                wildPet.ProtectionPower = 15f + (int)rarity * 10f;
                wildPet.PurificationPower = 10f + (int)rarity * 8f;
                
                // 设置捕捉相关属性
                wildPet.CaptureDifficulty = 1.0f - (float)rarity * 0.1f; // 稀有度越高越难捕捉
                wildPet.CanAbsorbSoul = true;
                wildPet.SoulAbsorptionRate = 0.5f + (int)rarity * 0.1f;
                
                // 创建野外神宠对象
                WildPet wild = new WildPet(wildPet, position);
                
                // 添加到列表
                wildPets.Add(wild);
                
                Debug.Log($"在位置 {position} 生成了野生神宠: {wildPet.Name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成野外神宠时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 攻击野生神宠（削弱其生命值以达到捕捉条件）
        /// </summary>
        public bool AttackWildPet(int petId, float damage, out string message)
        {
            message = "";
            
            for (int i = 0; i < wildPets.Count; i++)
            {
                if (wildPets[i].petData.Id == petId)
                {
                    WildPet updatedPet = wildPets[i];
                    updatedPet.remainingHP -= damage;
                    
                    if (updatedPet.remainingHP <= 0)
                    {
                        // 宠物死亡，掉落物品
                        message = ProcessWildPetDeath(i);
                        return false; // 宠物已死亡，不能捕捉
                    }
                    
                    // 如果生命值低于一定阈值，变为可捕捉状态
                    float captureThreshold = wildPets[i].petData.Health * 0.3f; // 30%生命值以下可捕捉
                    if (updatedPet.remainingHP <= captureThreshold)
                    {
                        updatedPet.isCapturable = true;
                        message = "宠物生命值已削弱，现在可以尝试捕捉！";
                    }
                    else
                    {
                        message = $"宠物受到 {damage} 点伤害，剩余生命值: {updatedPet.remainingHP}";
                    }
                    
                    wildPets[i] = updatedPet;
                    return true;
                }
            }
            
            message = "未找到指定的野生宠物";
            return false;
        }

        /// <summary>
        /// 尝试捕捉野生神宠
        /// </summary>
        public CaptureResult AttemptCapture(int petId, MythicalPetData capturerPet = null)
        {
            for (int i = 0; i < wildPets.Count; i++)
            {
                if (wildPets[i].petData.Id == petId)
                {
                    WildPet wildPet = wildPets[i];
                    
                    if (!wildPet.isCapturable)
                    {
                        return new CaptureResult(false, null, "宠物生命值过高，无法捕捉！", null);
                    }
                    
                    // 计算捕捉成功率
                    float successRate = GetCaptureSuccessRate(petId, capturerPet);
                    
                    if (Random.value <= successRate)
                    {
                        // 捕捉成功
                        MythicalPetData capturedPet = wildPet.petData;
                        
                        // 从野生列表中移除
                        wildPets.RemoveAt(i);
                        
                        // 生成掉落物
                        List<ItemDrop> drops = GenerateCaptureDrops(capturedPet);
                        
                        string message = $"成功捕捉到 {capturedPet.Name}！获得了资源奖励。";
                        
                        // 添加宠物到玩家收藏
                        if (GameManager.Instance?.PlayerData != null)
                        {
                            GameManager.Instance.PlayerData.AddPet(capturedPet);
                        }
                        
                        return new CaptureResult(true, capturedPet, message, drops);
                    }
                    else
                    {
                        return new CaptureResult(false, null, "捕捉失败！宠物逃脱了！", null);
                    }
                }
            }
            
            return new CaptureResult(false, null, "未找到指定的野生宠物", null);
        }

        /// <summary>
        /// 生成捕捉掉落物
        /// </summary>
        private List<ItemDrop> GenerateCaptureDrops(MythicalPetData capturedPet)
        {
            List<ItemDrop> drops = new List<ItemDrop>();
            
            // 固定掉落：金币
            drops.Add(new ItemDrop(ItemDrop.ItemType.Coin, 0, Random.Range(10, 50)));
            
            // 随机掉落：灵魂
            if (Random.value < 0.7f)
            {
                drops.Add(new ItemDrop(ItemDrop.ItemType.Soul, 
                    (float)Random.Range(1, 5), 1));
            }
            
            // 随机掉落：宠物蛋
            if (Random.value < 0.2f)
            {
                drops.Add(new ItemDrop(ItemDrop.ItemType.PetEgg, 0, 1));
            }
            
            // 根据宠物稀有度掉落额外资源
            int extraDrops = (int)capturedPet.Rarity;
            if (extraDrops > 0)
            {
                drops.Add(new ItemDrop(ItemDrop.ItemType.Resource, 0, extraDrops));
            }
            
            return drops;
        }

        /// <summary>
        /// 获取捕捉成功率
        /// </summary>
        public float GetCaptureSuccessRate(int petId, MythicalPetData capturerPet = null)
        {
            for (int i = 0; i < wildPets.Count; i++)
            {
                if (wildPets[i].petData.Id == petId)
                {
                    MythicalPetData targetPet = wildPets[i].petData;
                    float remainingHealthRatio = wildPets[i].remainingHP / targetPet.Health;
                    
                    // 基础成功率受宠物稀有度和生命值影响
                    float baseRate = 1.0f - targetPet.CaptureDifficulty;
                    float healthFactor = 1.0f - remainingHealthRatio; // 生命值越低成功率越高
                    float playerFactor = 0f;
                    
                    // 如果有捕捉者宠物，增加成功率
                    if (capturerPet != null)
                    {
                        playerFactor = (capturerPet.AttackPower + capturerPet.DivinePower) * 0.001f;
                    }
                    
                    float finalRate = Mathf.Clamp01(baseRate + healthFactor + playerFactor);
                    return finalRate;
                }
            }
            
            return 0f; // 未找到宠物
        }

        /// <summary>
        /// 检查指定位置是否有野生神宠
        /// </summary>
        public bool HasWildPetAtPosition(Vector3 position, float radius = 1.0f)
        {
            foreach (var wildPet in wildPets)
            {
                if (Vector3.Distance(position, wildPet.position) < radius)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 野怪死亡处理（掉落物生成）
        /// </summary>
        private string ProcessWildPetDeath(int wildPetIndex)
        {
            if (wildPetIndex < 0 || wildPetIndex >= wildPets.Count)
            {
                return "无效的宠物索引";
            }
            
            MythicalPetData deadPet = wildPets[wildPetIndex].petData;
            
            // 从列表中移除
            wildPets.RemoveAt(wildPetIndex);
            
            // 生成死亡掉落物
            List<ItemDrop> drops = GenerateDeathDrops(deadPet);
            
            // 将掉落物给予玩家
            GiveDropsToPlayer(drops);
            
            string message = $"{deadPet.Name} 被击败了！获得了战利品。";
            Debug.Log(message);
            
            return message;
        }

        /// <summary>
        /// 生成死亡掉落物
        /// </summary>
        private List<ItemDrop> GenerateDeathDrops(MythicalPetData deadPet)
        {
            List<ItemDrop> drops = new List<ItemDrop>();
            
            // 金币掉落
            int coinDrop = Random.Range(15, 40);
            drops.Add(new ItemDrop(ItemDrop.ItemType.Coin, coinDrop, 1));
            
            // 灵魂掉落
            float soulDrop = Random.Range(1.0f, 3.0f);
            drops.Add(new ItemDrop(ItemDrop.ItemType.Soul, soulDrop, 1));
            
            // 神石掉落（较低概率）
            if (Random.value < 0.3f)
            {
                int gemDrop = Random.Range(1, 3);
                drops.Add(new ItemDrop(ItemDrop.ItemType.Resource, gemDrop, 1));
            }
            
            return drops;
        }

        /// <summary>
        /// 将掉落物给予玩家
        /// </summary>
        private void GiveDropsToPlayer(List<ItemDrop> drops)
        {
            if (GameManager.Instance?.PlayerData == null) return;
            
            var playerData = GameManager.Instance.PlayerData;
            
            foreach (var drop in drops)
            {
                switch (drop.type)
                {
                    case ItemDrop.ItemType.Coin:
                        playerData.AddCoins((int)drop.value * drop.quantity);
                        break;
                    case ItemDrop.ItemType.Soul:
                        playerData.AddSouls(drop.value * drop.quantity);
                        break;
                    case ItemDrop.ItemType.Resource:
                        playerData.AddGems((int)drop.value * drop.quantity);
                        break;
                }
            }
        }

        /// <summary>
        /// 检查捕捉阶段是否完成
        /// </summary>
        public bool IsCapturePhaseComplete()
        {
            return !capturePhaseActive || timeInCapturePhase >= capturePhaseDuration;
        }

        /// <summary>
        /// 获取捕捉阶段剩余时间
        /// </summary>
        public float GetTimeRemaining()
        {
            return Mathf.Max(0, capturePhaseDuration - timeInCapturePhase);
        }

        /// <summary>
        /// 获取当前野外神宠列表
        /// </summary>
        public List<WildPet> GetWildPets()
        {
            return wildPets;
        }

        /// <summary>
        /// 更新捕捉系统
        /// </summary>
        public void UpdateCapture(float deltaTime)
        {
            // 更新野生宠物的消失计时
            for (int i = wildPets.Count - 1; i >= 0; i--)
            {
                WildPet updatedPet = wildPets[i];
                updatedPet.timeUntilDespawn -= deltaTime;
                
                if (updatedPet.timeUntilDespawn <= 0)
                {
                    // 宠物消失
                    wildPets.RemoveAt(i);
                    Debug.Log($"野生宠物 {updatedPet.petData.Name} 消失了");
                }
                else
                {
                    wildPets[i] = updatedPet;
                }
            }
        }

        /// <summary>
        /// 重置捕捉系统
        /// </summary>
        public void Reset()
        {
            InitializeExplorationSystem();
        }
    }
}