using Friflo.Engine.ECS;
using F.ECS.Components;

namespace F.ECS.Systems;

public class TokenProcessingSystem
{
    private readonly EntityStore _store;
    
    public TokenProcessingSystem(EntityStore store)
    {
        _store = store;
    }
    
    public void Update()
    {
        var query = _store.Query<TokenBlockComponent, TokenValueComponent>();
        
        query.ForEachEntity((ref TokenBlockComponent block, ref TokenValueComponent value, Entity entity) =>
        {
            // If we have a target block and we're not moving, process the token
            if (block.TargetBlock != null && block.CurrentBlock != block.TargetBlock)
            {
                // Add to processed blocks
                block.ProcessedBlocks.Add(block.CurrentBlock!);
                
                // Update current block
                block.CurrentBlock = block.TargetBlock;
                block.TargetBlock = null;
                
                // Let the block process the token's value
                if (block.CurrentBlock != null)
                {
                    // We'll need to implement a way to process values through blocks
                    // block.CurrentBlock.ProcessToken(...);
                }
            }
        });
    }
} 