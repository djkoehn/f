flowchart LR
Animation(Animation)
Animation(Animation)
Animation(Animation)
AudioManager(AudioManager)
BaseBlock(BaseBlock)
Block(Block)
BlockConfig(BlockConfig)
BlockFactory(BlockFactory)
BlockInteractionManager(BlockInteractionManager)
BlockLayer(BlockLayer)
BlockMetadata(BlockMetadata)
BlockMetadataFile(BlockMetadataFile)
BlockPositionsUpdatedEventHandler(BlockPositionsUpdatedEventHandler)
BlockReturn(BlockReturn)
BlockSoundPlayer(BlockSoundPlayer)
BlockState(BlockState)
ConnectionFactory(ConnectionFactory)
ConnectionHelper(ConnectionHelper)
ConnectionManager(ConnectionManager)
ConnectionPipe(ConnectionPipe)
ConnectionValidator(ConnectionValidator)
DragHelper(DragHelper)
Easing(Easing)
GameManager(GameManager)
GameStateManager(GameStateManager)
HelperFunnel(HelperFunnel)
IBlock(IBlock)
IBlockContainer(IBlockContainer)
Input(Input)
InputHelper(InputHelper)
Interaction(Interaction)
Inventory(Inventory)
InventoryReadyEventHandler(InventoryReadyEventHandler)
Layers(Layers)
Layout(Layout)
Movement(Movement)
MovementCompleteEventHandler(MovementCompleteEventHandler)
MovementStartEventHandler(MovementStartEventHandler)
Output(Output)
PipeBulge(PipeBulge)
PipeConfig(PipeConfig)
PipeCurveCalculator(PipeCurveCalculator)
PipeRewiringHelper(PipeRewiringHelper)
PipeSelector(PipeSelector)
PipeVisuals(PipeVisuals)
PositionChangedHandler(PositionChangedHandler)
ReturnCompletedEventHandler(ReturnCompletedEventHandler)
SceneInitializer(SceneInitializer)
ScriptGlobals(ScriptGlobals)
Token(Token)
TokenConfig(TokenConfig)
TokenFactory(TokenFactory)
TokenManager(TokenManager)
TokenSoundPlayer(TokenSoundPlayer)
TokenVisuals(TokenVisuals)
Toolbar(Toolbar)
ToolbarBlockContainer(ToolbarBlockContainer)
ToolbarBlockManager(ToolbarBlockManager)
ToolbarBlockSlot(ToolbarBlockSlot)
ToolbarConfig(ToolbarConfig)
ToolbarContainerAnimation(ToolbarContainerAnimation)
ToolbarHelper(ToolbarHelper)
ToolbarHoverAnimation(ToolbarHoverAnimation)
ToolbarVisuals(ToolbarVisuals)
TriggerBlock(TriggerBlock)
TweenHelper(TweenHelper)
Visual(Visual)
Visual(Visual)
Visual(Visual)
ZIndexConfig(ZIndexConfig)

AudioManager  -->  AudioManager 
AudioManager  -->  BlockSoundPlayer 
AudioManager  -->  TokenSoundPlayer 
BaseBlock  -->  BlockConfig 
BaseBlock  -->  BlockMetadata 
BaseBlock  -->  BlockMetadata 
BaseBlock  -->  BlockState 
BaseBlock  -->  BlockState 
BaseBlock  -->  ConnectionManager 
BaseBlock  -->  ConnectionPipe 
BaseBlock  -->  IBlock 
BaseBlock  -->  ScriptGlobals 
BaseBlock  -->  Token 
BlockFactory  -->  BaseBlock 
BlockFactory  -->  BaseBlock 
BlockFactory  -->  BlockMetadata 
BlockFactory  --*  GameManager 
BlockInteractionManager  -->  BaseBlock 
BlockInteractionManager  -->  BaseBlock 
BlockInteractionManager  -->  ConnectionManager 
BlockInteractionManager  -->  GameManager 
BlockInteractionManager  -->  HelperFunnel 
BlockMetadata  -->  BlockMetadata 
BlockMetadata  -->  BlockMetadata 
BlockMetadata  -->  BlockMetadataFile 
BlockMetadataFile  -->  BlockMetadata 
BlockMetadataFile  -->  BlockMetadata 
BlockReturn  -->  BaseBlock 
BlockReturn  -->  BaseBlock 
BlockReturn  --*  BaseBlock 
BlockReturn  -->  BlockReturn 
BlockReturn  -->  Easing 
BlockReturn  -->  Layers 
BlockReturn  -->  ReturnCompletedEventHandler 
BlockReturn  -->  ZIndexConfig 
ConnectionFactory  -->  ConnectionHelper 
ConnectionFactory  -->  ConnectionPipe 
ConnectionFactory  -->  ConnectionPipe 
ConnectionFactory  -->  IBlock 
ConnectionFactory  -->  ToolbarBlockContainer 
ConnectionHelper  -->  BaseBlock 
ConnectionHelper  -->  ConnectionManager 
ConnectionHelper  -->  ConnectionPipe 
ConnectionHelper  -->  GameManager 
ConnectionHelper  -->  IBlock 
ConnectionManager  -->  BaseBlock 
ConnectionManager  -->  ConnectionFactory 
ConnectionManager  -->  ConnectionPipe 
ConnectionManager  -->  ConnectionPipe 
ConnectionManager  -->  ConnectionValidator 
ConnectionManager  -->  IBlock 
ConnectionManager  -->  Input 
ConnectionManager  -->  Layers 
ConnectionManager  -->  Output 
ConnectionManager  -->  PipeRewiringHelper 
ConnectionManager  -->  PipeSelector 
ConnectionManager  -->  ToolbarHoverAnimation 
ConnectionManager  -->  ZIndexConfig 
ConnectionPipe  -->  IBlock 
ConnectionPipe  -->  IBlock 
ConnectionPipe  -->  Interaction 
ConnectionPipe  -->  Layers 
ConnectionPipe  -->  PipeConfig 
ConnectionPipe  -->  PipeCurveCalculator 
ConnectionPipe  -->  Visual 
ConnectionPipe  -->  ZIndexConfig 
ConnectionValidator  -->  IBlock 
DragHelper  -->  BaseBlock 
DragHelper  -->  DragHelper 
DragHelper  -->  Layers 
DragHelper  -->  ZIndexConfig 
GameManager  -->  BaseBlock 
GameManager  -->  BaseBlock 
GameManager  -->  BlockConfig 
GameManager  -->  BlockFactory 
GameManager  -->  BlockFactory 
GameManager  -->  BlockInteractionManager 
GameManager  -->  BlockMetadata 
GameManager  -->  ConnectionManager 
GameManager  -->  ConnectionManager 
GameManager  -->  GameManager 
GameManager  -->  GameStateManager 
GameManager  -->  Inventory 
GameManager  -->  TokenManager 
GameManager  -->  TokenManager 
GameStateManager  --*  Inventory 
HelperFunnel  -->  DragHelper 
HelperFunnel  -->  DragHelper 
HelperFunnel  -->  HelperFunnel 
HelperFunnel  -->  TweenHelper 
HelperFunnel  -->  TweenHelper 
IBlock  -->  Token 
Input  -->  BlockConfig 
Input  -->  ConnectionManager 
Input  -->  IBlock 
Input  -->  Token 
Input  -->  TokenManager 
InputHelper  -->  BaseBlock 
InputHelper  -->  BlockInteractionManager 
InputHelper  -->  BlockState 
InputHelper  -->  ConnectionHelper 
InputHelper  -->  DragHelper 
InputHelper  -->  HelperFunnel 
InputHelper  -->  IBlock 
InputHelper  -->  Layers 
InputHelper  -->  ToolbarBlockContainer 
InputHelper  -->  ToolbarHelper 
InputHelper  -->  ZIndexConfig 
Inventory  -->  BaseBlock 
Inventory  -->  BlockMetadata 
Inventory  -->  BlockMetadata 
Inventory  -->  InventoryReadyEventHandler 
Output  -->  BlockConfig 
Output  -->  IBlock 
Output  -->  Token 
PipeCurveCalculator  -->  ConnectionHelper 
PipeRewiringHelper  -->  BaseBlock 
PipeRewiringHelper  -->  ConnectionFactory 
PipeRewiringHelper  -->  ConnectionManager 
PipeRewiringHelper  -->  ConnectionPipe 
PipeRewiringHelper  -->  IBlock 
PipeSelector  -->  ConnectionPipe 
PipeSelector  -->  ConnectionPipe 
PipeVisuals  -->  Animation 
PipeVisuals  -->  PipeBulge 
PipeVisuals  -->  PipeConfig 
PipeVisuals  -->  Visual 
SceneInitializer  -->  Layers 
SceneInitializer  -->  ZIndexConfig 
ScriptGlobals  -->  BaseBlock 
ScriptGlobals  -->  BaseBlock 
ScriptGlobals  --*  BaseBlock 
ScriptGlobals  -->  ConnectionManager 
ScriptGlobals  -->  ConnectionManager 
ScriptGlobals  --*  ConnectionManager 
ScriptGlobals  -->  Token 
ScriptGlobals  -->  Token 
ScriptGlobals  --*  Token 
Token  -->  ConnectionPipe 
Token  -->  IBlock 
Token  -->  IBlock 
Token  -->  TokenVisuals 
TokenFactory  -->  IBlock 
TokenFactory  -->  Token 
TokenFactory  -->  Token 
TokenManager  -->  AudioManager 
TokenManager  -->  ConnectionManager 
TokenManager  --*  ConnectionManager 
TokenManager  -->  IBlock 
TokenManager  -->  Input 
TokenManager  -->  Token 
TokenManager  -->  TokenFactory 
TokenVisuals  -->  Animation 
TokenVisuals  -->  MovementCompleteEventHandler 
TokenVisuals  -->  MovementStartEventHandler 
TokenVisuals  -->  TokenConfig 
Toolbar  -->  Animation 
Toolbar  -->  BaseBlock 
Toolbar  -->  BlockFactory 
Toolbar  -->  BlockReturn 
Toolbar  -->  ConnectionManager 
Toolbar  -->  GameManager 
Toolbar  -->  HelperFunnel 
Toolbar  -->  IBlock 
Toolbar  -->  Inventory 
Toolbar  -->  Layers 
Toolbar  -->  ToolbarBlockContainer 
Toolbar  -->  ToolbarConfig 
Toolbar  -->  ToolbarHelper 
Toolbar  -->  ToolbarHoverAnimation 
Toolbar  -->  ToolbarVisuals 
Toolbar  -->  ZIndexConfig 
ToolbarBlockContainer  -->  BaseBlock 
ToolbarBlockContainer  -->  HelperFunnel 
ToolbarBlockContainer  -->  Layers 
ToolbarBlockContainer  -->  Toolbar 
ToolbarBlockContainer  -->  ToolbarHelper 
ToolbarBlockContainer  -->  ToolbarVisuals 
ToolbarBlockContainer  -->  ZIndexConfig 
ToolbarBlockManager  -->  BaseBlock 
ToolbarBlockManager  -->  BlockMetadata 
ToolbarBlockManager  -->  BlockPositionsUpdatedEventHandler 
ToolbarBlockManager  -->  BlockState 
ToolbarBlockManager  -->  ConnectionManager 
ToolbarBlockManager  -->  ConnectionPipe 
ToolbarBlockManager  -->  DragHelper 
ToolbarBlockManager  -->  GameManager 
ToolbarBlockManager  -->  HelperFunnel 
ToolbarBlockManager  -->  IBlock 
ToolbarBlockSlot  -->  BaseBlock 
ToolbarContainerAnimation  -->  BaseBlock 
ToolbarContainerAnimation  -->  Easing 
ToolbarContainerAnimation  -->  ToolbarContainerAnimation 
ToolbarHelper  -->  BaseBlock 
ToolbarHelper  -->  ConnectionManager 
ToolbarHelper  -->  ConnectionPipe 
ToolbarHelper  -->  IBlock 
ToolbarHelper  -->  Layers 
ToolbarHelper  -->  ToolbarBlockContainer 
ToolbarHelper  -->  ZIndexConfig 
ToolbarHoverAnimation  -->  Easing 
ToolbarHoverAnimation  -->  PositionChangedHandler 
ToolbarHoverAnimation  -->  ToolbarHoverAnimation 
ToolbarVisuals  -->  ToolbarHoverAnimation 
