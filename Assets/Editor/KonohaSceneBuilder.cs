using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class KonohaSceneBuilder
{
    const string ScenePath = "Assets/Scenes/KonohaVillage.unity";
    const string BuildVersionPath = "Assets/Konoha/KonohaBuildVersion.txt";
    const string BuildVersion = "23";
    static Material grass, road, plaster, roof, wood, stone, red, water, riverWater, leaf, grassTufts, dark;
    static Material sage, blush, blueRoof, hokageRed, hokageRoof, hokageDome, hokageGlass, ochre, sandstone;
    static Material hokageRockReference;
    static Texture2D hokageRockTexture;
    static readonly System.Collections.Generic.List<Vector2> housePositions = new System.Collections.Generic.List<Vector2>();
    // World orientation: north is +Z, south is -Z, east is +X, west is -X.
    // Four northern compounds preserve the reference map; Akimichi extends the west/southwest clan quarter.
    static readonly Vector2[] clanDistrictCenters={new Vector2(-205,180),new Vector2(-68,184),new Vector2(70,184),new Vector2(207,180),new Vector2(-205,56),new Vector2(-90,55),new Vector2(205,56),new Vector2(-205,-190)};
    static Transform activeLayerRoot, geographyLayer, clanLayer, urbanLayer, ninjaLayer, outskirtsLayer, characterLayer;

    static KonohaSceneBuilder()
    {
        EditorApplication.delayCall += BuildWhenEditorIsReady;
    }
    static void BuildWhenEditorIsReady()
    {
        if(!File.Exists(ScenePath) || (File.Exists(BuildVersionPath) && File.ReadAllText(BuildVersionPath).Trim()==BuildVersion))return;
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.playModeStateChanged-=BuildAfterPlay;EditorApplication.playModeStateChanged+=BuildAfterPlay;
            Debug.Log("Konoha update queued until Play mode ends.");return;
        }
        BuildAndSave();
    }
    static void BuildAfterPlay(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredEditMode)return;
        EditorApplication.playModeStateChanged-=BuildAfterPlay;EditorApplication.delayCall+=BuildWhenEditorIsReady;
    }

    [MenuItem("NARUTO MMORPG/Build Konoha Village")]
    public static void BuildAndSave()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.LogWarning("Stop Play mode before rebuilding the Konoha scene.");return;}
        grass = Mat("Grass", new Color(.34f,.48f,.26f)); road = Mat("Packed earth", new Color(.55f,.42f,.29f));
        plaster = Mat("Warm plaster", new Color(.88f,.79f,.61f)); roof = Mat("Terracotta roofs", new Color(.78f,.31f,.16f));
        wood = Mat("Dark timber", new Color(.25f,.14f,.10f)); stone = Mat("Gate stone", new Color(.54f,.53f,.48f));
        red = Mat("Gate vermilion", new Color(.68f,.16f,.11f)); water = Mat("Pond", new Color(.18f,.42f,.48f));
        leaf = Mat("Forest green", new Color(.18f,.34f,.19f)); leaf.SetFloat("_WindStrength",.12f);
        grassTufts=Mat("Hand painted grass tufts",Color.white);var grassShader=Shader.Find("Konoha/GrassWind");if(grassShader!=null)grassTufts.shader=grassShader;
        if(grassTufts.HasProperty("_BaseMap"))grassTufts.SetTexture("_BaseMap",StylizedTexture("Grass"));
        if(grassTufts.HasProperty("_BaseColor"))grassTufts.SetColor("_BaseColor",Color.white);
        if(grassTufts.HasProperty("_WindStrength"))grassTufts.SetFloat("_WindStrength",.18f);dark = Mat("Shadow", new Color(.13f,.12f,.11f));
        riverWater=Mat("Flowing river water",new Color(.16f,.50f,.59f));var flowShader=Shader.Find("Konoha/FlowingWater");if(flowShader!=null)riverWater.shader=flowShader;
        if(riverWater.HasProperty("_BaseMap"))riverWater.SetTexture("_BaseMap",StylizedTexture("Flowing river water"));
        if(riverWater.HasProperty("_BaseColor"))riverWater.SetColor("_BaseColor",new Color(.16f,.50f,.59f));
        if(riverWater.HasProperty("_FlowSpeed"))riverWater.SetFloat("_FlowSpeed",.11f);
        if(riverWater.HasProperty("_WaveStrength"))riverWater.SetFloat("_WaveStrength",.38f);
        EditorUtility.SetDirty(grassTufts);EditorUtility.SetDirty(riverWater);
        sage=Mat("Sage plaster",new Color(.62f,.73f,.52f));blush=Mat("Rose plaster",new Color(.82f,.56f,.45f));
        blueRoof=Mat("Indigo roof",new Color(.18f,.30f,.43f));
        hokageRoof=Mat("Hokage orange tiles",new Color(.95f,.49f,.17f),true);hokageDome=Mat("Hokage pale jade dome",new Color(.77f,.79f,.60f),true);
        hokageGlass=Mat("Hokage dark windows",new Color(.16f,.28f,.31f),true);hokageRed=Mat("Hokage red",new Color(.79f,.17f,.14f),true);
        ochre=Mat("Ochre plaster",new Color(.84f,.68f,.38f));sandstone=Mat("Hokage sandstone",new Color(.68f,.51f,.35f));
        hokageRockReference=HokageRockReferenceMaterial();
        var s = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var worldRoot=new GameObject("Konoha - World by Layers").transform;
        worldRoot.localScale=new Vector3(1.28f,1f,1.42f);
        geographyLayer=CreateLayer(worldRoot,"01 - Geography");clanLayer=CreateLayer(worldRoot,"02 - Clan Compounds");
        urbanLayer=CreateLayer(worldRoot,"03 - Urban Center");ninjaLayer=CreateLayer(worldRoot,"04 - Ninja Infrastructure");
        outskirtsLayer=CreateLayer(worldRoot,"05 - Outskirts");characterLayer=CreateLayer(worldRoot,"Characters and Cameras");
        SetActiveLayer(geographyLayer);
        RenderSettings.ambientLight = new Color(.72f,.75f,.68f);
        RenderSettings.fog = true; RenderSettings.fogColor = new Color(.69f,.76f,.79f); RenderSettings.fogDensity = .0025f;
        var sun = new GameObject("Afternoon Sun"); var light = sun.AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.25f; sun.transform.rotation=Quaternion.Euler(48,-32,0);
        Register(sun);
        CreateVillageTerrain();
        // Outer defensive wall and four entry gates.
        Wall("North wall beyond Hokage Rock",0,340,600,8);
        Wall("South wall west",-145,-280,270,8);Wall("South wall east",145,-280,270,8);
        Wall("East wall north",280,172,8,328);Wall("East wall south",280,-142,8,268);
        Wall("West wall north",-280,172,8,328);Wall("West wall south",-280,-142,8,268);
        foreach(float x in new[]{-220f,-110f,110f,220f})WallWatchtower("South wall watchtower",x,-280);
        foreach(float z in new[]{-190f,-95f,95f,190f,285f}){WallWatchtower("East wall watchtower",280,z);WallWatchtower("West wall watchtower",-280,z);}
        foreach(float x in new[]{-220f,-110f,110f,220f})WallWatchtower("North wall watchtower",x,340);
        Gate("South village gate",0,-278,0);Gate("East village gate",278,0,90);Gate("West village gate",-278,0,-90);
        VillageRiver();CreateHokageCliff();CreateEasternWaterfall();
        // The forest belt now surrounds the expanded wall and leaves open approaches at the gates.
        SetActiveLayer(outskirtsLayer);
        for(int ring=0;ring<4;ring++)for(int i=0;i<240;i++)
        {
            float a=(i+(ring%2)*.5f)*Mathf.PI*2/240f;float r=500+ring*44+Mathf.Sin(i*2.17f+ring)*9;
            Vector3 p=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
            if(Mathf.Abs(p.x)<28f&&p.z< -275f)continue;
            if(Mathf.Abs(p.z)<26f&&Mathf.Abs(p.x)>275f)continue;
            Tree("Konoha outer forest",p,1.15f+(i%5)*.18f);
        }
        // Main streets and cross streets.
        SetActiveLayer(urbanLayer);
        CreateNpcCrowd();
        Cube("South Main Avenue from main gate", new Vector3(0,.01f,-128), new Vector3(20,.12f,300), road);
        Cube("North Avenue below Hokage plaza", new Vector3(0,.01f,18), new Vector3(20,.12f,76), road);
        RiverApproachLane("East lane",54,10,-245,180);RiverApproachLane("West lane",-55,10,-245,180);
        RiverApproachLane("North-east river crossing",220,10,-245,180);RiverApproachLane("North-west river crossing",-220,10,-245,180);
        // Homes loosely follow the road network but leave varied gaps and keep the river clear.
        PopulateResidentialBlocks();
        SetActiveLayer(clanLayer);CreateClanDistricts();
        SetActiveLayer(urbanLayer);
        // Landmark buildings and village life.
        HokageOffice();MarketStalls();CreateUrbanDistricts();
        SetActiveLayer(ninjaLayer);CreateNinjaInfrastructure();
        SetActiveLayer(urbanLayer);
        for(int i=0;i<24;i++)Cube("Hokage Rock visitor stair",new Vector3(-132,.14f+i*.16f,182+i*2.05f),new Vector3(20,.28f,2.6f),sandstone);
        // Hokage residence, academy, market, and memorial garden.
        House(-36,18,0,"Ninja Academy",1.5f); House(34,19,1,"Ichiraku Ramen",.8f);
        Cube("Academy courtyard",new Vector3(-35,.05f,5),new Vector3(35,.14f,22),road);
        SetActiveLayer(outskirtsLayer);
        Cube("Training field",new Vector3(-180,.06f,-350),new Vector3(48,.16f,40),road);
        for(int i=0;i<8;i++){ float a=i*Mathf.PI/4; Tree("Training field tree",new Vector3(-180+Mathf.Cos(a)*29,0,-350+Mathf.Sin(a)*24)); }
        CreateOutskirts();
        BuildGrassTufts();
        SetActiveLayer(urbanLayer);Pond();MemorialStone();
        // Lanterns along the central avenue.
        for(int z=-84;z<=84;z+=18){ Lantern(-10,z); Lantern(10,z); }
        // Player start facing toward the Hokage monument.
        SetActiveLayer(characterLayer);
        var player=new GameObject("Player - WASD to walk");Register(player);player.transform.position=new Vector3(0,1.1f,-140);
        var capsule=player.AddComponent<CharacterController>(); capsule.height=1.8f; capsule.radius=.38f; capsule.center=new Vector3(0,.9f,0);
        player.AddComponent<KonohaWalker>();
        var cam=new GameObject("Follow Camera");Register(cam);cam.tag="MainCamera"; var camera=cam.AddComponent<Camera>(); camera.fieldOfView=65; cam.AddComponent<AudioListener>();
        var follow=cam.AddComponent<KonohaFollowCamera>(); follow.target=player.transform; cam.transform.position=player.transform.position+new Vector3(0,4,-7); cam.transform.LookAt(player.transform.position+Vector3.up*1.2f);
        // Keep the scene self-contained and immediately playable.
        EditorSceneManager.SaveScene(s,ScenePath);
        File.WriteAllText(BuildVersionPath,BuildVersion);
        AssetDatabase.Refresh();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Selection.activeGameObject=player;
        FocusKonohaMap();
        Debug.Log("Konoha village generated at Assets/Scenes/KonohaVillage.unity. Press Play; walk with WASD and look with the mouse.");
    }
    [MenuItem("NARUTO MMORPG/Encuadrar mapa de Konoha")]
    public static void FocusKonohaMap()
    {
        var view=SceneView.lastActiveSceneView;
        if(view==null)return;
        view.pivot=new Vector3(0,48,0);view.rotation=Quaternion.Euler(52,0,0);view.size=840;view.orthographic=false;view.Repaint();
    }
    static Transform CreateLayer(Transform parent,string name)
    {
        var layer=new GameObject(name).transform;layer.SetParent(parent,false);return layer;
    }
    static void SetActiveLayer(Transform layer){activeLayerRoot=layer;}
    static void Register(GameObject item)
    {
        if(item!=null&&activeLayerRoot!=null&&item.transform.parent!=activeLayerRoot)item.transform.SetParent(activeLayerRoot,true);
    }
    static Material HokageRockReferenceMaterial()
    {
        const string texturePath="Assets/Konoha/Textures/HokageRockReferenceV2.png";
        hokageRockTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if(hokageRockTexture==null){Debug.LogError("Hokage Rock reference image was not imported: "+texturePath);return sandstone;}
        var importer=AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if(importer!=null)
        {
            bool changed=false;
            if(importer.textureCompression!=TextureImporterCompression.Uncompressed){importer.textureCompression=TextureImporterCompression.Uncompressed;changed=true;}
            if(!importer.mipmapEnabled){importer.mipmapEnabled=true;changed=true;}
            if(importer.filterMode!=FilterMode.Trilinear){importer.filterMode=FilterMode.Trilinear;changed=true;}
            if(importer.anisoLevel!=8){importer.anisoLevel=8;changed=true;}
            if(importer.wrapMode!=TextureWrapMode.Clamp){importer.wrapMode=TextureWrapMode.Clamp;changed=true;}
            if(!importer.isReadable){importer.isReadable=true;changed=true;}
            if(changed){importer.SaveAndReimport();hokageRockTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);}
        }
        const string materialPath="Assets/Konoha/Materials/HokageRockReference.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null)
        {
            var shader=Shader.Find("Konoha/ReferenceNoFog")??Shader.Find("Universal Render Pipeline/Unlit")??Shader.Find("Unlit/Texture")??Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            material=new Material(shader){name="Hokage Rock exact image reference"};AssetDatabase.CreateAsset(material,materialPath);
        }
        var referenceShader=Shader.Find("Konoha/ReferenceNoFog");if(referenceShader!=null)material.shader=referenceShader;
        if(material.HasProperty("_BaseMap"))material.SetTexture("_BaseMap",hokageRockTexture);
        if(material.HasProperty("_MainTex"))material.SetTexture("_MainTex",hokageRockTexture);
        if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",Color.white);
        material.color=Color.white;material.enableInstancing=true;EditorUtility.SetDirty(material);return material;
    }
    static Material Mat(string n,Color c){
        const string folder="Assets/Konoha/Materials";
        if(!AssetDatabase.IsValidFolder("Assets/Konoha")) AssetDatabase.CreateFolder("Assets","Konoha");
        if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Konoha","Materials");
        string path=folder+"/"+n.Replace(" ","")+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard")){name=n};AssetDatabase.CreateAsset(m,path);}
        var toon=Shader.Find("Konoha/AnimeToon");
        m.shader=toon??Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");m.color=c;m.enableInstancing=true;
        var tex=StylizedTexture(n);if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",tex);if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",tex);
        if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);
        if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.08f);EditorUtility.SetDirty(m);return m;
    }
    static Material Mat(string n,Color c,bool inkOutline){
        const string folder="Assets/Konoha/Materials";
        if(!AssetDatabase.IsValidFolder("Assets/Konoha")) AssetDatabase.CreateFolder("Assets","Konoha");
        if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Konoha","Materials");
        string path=folder+"/"+n.Replace(" ","")+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard")){name=n};AssetDatabase.CreateAsset(m,path);}
        var toon=Shader.Find(inkOutline?"Konoha/AnimeToonOutline":"Konoha/AnimeToon");m.shader=toon??Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");m.color=c;m.enableInstancing=true;
        var tex=StylizedTexture(n);if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",tex);if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",tex);if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);
        EditorUtility.SetDirty(m);return m;
    }
    static Texture2D StylizedTexture(string name)
    {
        const string folder="Assets/Konoha/Textures";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Konoha","Textures");
        string path=folder+"/"+name.Replace(" ","")+"_AnimeStorm_v3.asset";var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(tex!=null)return tex;
        int kind=name.Contains("Forest")?9:name.Contains("Grass")?0:name.Contains("earth")?1:name.Contains("plaster")||name.Contains("dome")?2:name.Contains("roof")||name.Contains("tiles")?3:name.Contains("timber")||name.Contains("wood")?4:name.Contains("stone")||name.Contains("Shadow")?5:name.Contains("water")||name.Contains("Pond")?6:name.Contains("glass")||name.Contains("windows")?8:7;
        const int size=256;tex=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name+" anime brush texture",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
        var pixels=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float h=Mathf.Abs(Mathf.Sin((x+kind*13)*12.9898f+(y-kind*7)*78.233f)*43758.5453f);h-=Mathf.Floor(h);
            float shade=.78f+h*.28f;bool line=false;bool highlight=false;
            if(kind==0){line=(x*5+y*3+(int)(h*9))%61<3;highlight=!line&&h>.91f&&(x+y*2)%13<3;}
            if(kind==1)line=(x%24<2)||(y%24<2);
            if(kind==2)line=(x%39==0&&y%31<18)||(y%53==0&&x%71<40);
            if(kind==3)line=(y%16<3)||((x+(y/16%2)*9)%29<3);
            if(kind==4)line=(y%15<3)||(x%57==0&&y%42<20);
            if(kind==5)line=(x%33==0&&y%29<16)||h<.13f;
            if(kind==6){line=(y+(int)(Mathf.Sin(x*.07f)*5))%29<3;highlight=!line&&Mathf.Sin(x*.11f+y*.055f)> .94f;}
            if(kind==7)line=(x+y)%31<2;
            if(kind==8)line=(x%17<3)||(y%23<3);
            if(kind==9){line=((x/18+y/13)%5==0&&h<.45f)||(x%31<2&&h>.7f);highlight=!line&&h>.91f;}
            if(line)shade*=kind==3?.52f:.62f;if(highlight)shade=Mathf.Min(1.18f,shade*1.24f);
            pixels[y*size+x]=new Color(shade,shade,shade,1);
        }
        tex.SetPixels(pixels);tex.Apply();AssetDatabase.CreateAsset(tex,path);return tex;
    }
    static GameObject Cube(string n,Vector3 p,Vector3 scale,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.position=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;Register(g);return g;}
    static GameObject LocalCube(GameObject parent,string n,Vector3 p,Vector3 scale,Material m){var g=Cube(n,Vector3.zero,scale,m);g.transform.SetParent(parent.transform,false);g.transform.localPosition=p;return g;}
    static void CreateVillageTerrain()
    {
        CreateLandStrip("Village south grassland",false);CreateLandStrip("Village north grassland",true);
    }
    static void CreateLandStrip(string name,bool north)
    {
        const int segments=128;const float xMin=-500f,xMax=500f,outer=500f,edgeOffset=16f,topY=-.05f;
        var vertices=new Vector3[(segments+1)*2];var uv=new Vector2[vertices.Length];var triangles=new int[segments*6];
        for(int i=0;i<=segments;i++)
        {
            float u=i/(float)segments,x=Mathf.Lerp(xMin,xMax,u),center=RiverCenter(x);
            Vector3 inner=new Vector3(x,topY,center)+RiverSideVector(x)*(north?edgeOffset:-edgeOffset);
            vertices[i*2]=north?inner:new Vector3(x,topY,-outer);vertices[i*2+1]=north?new Vector3(x,topY,outer):inner;
            uv[i*2]=new Vector2(vertices[i*2].x*.3f,vertices[i*2].z*.3f);uv[i*2+1]=new Vector2(vertices[i*2+1].x*.3f,vertices[i*2+1].z*.3f);
        }
        int t=0;for(int i=0;i<segments;i++){int a=i*2,b=a+1,c=a+2,d=a+3;triangles[t++]=a;triangles[t++]=b;triangles[t++]=c;triangles[t++]=b;triangles[t++]=d;triangles[t++]=c;}
        var mesh=new Mesh();mesh.name=name+" cutaway mesh";mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
        var land=new GameObject(name+" - river channel opening");Register(land);land.AddComponent<MeshFilter>().sharedMesh=mesh;land.AddComponent<MeshRenderer>().sharedMaterial=grass;land.AddComponent<MeshCollider>().sharedMesh=mesh;
    }
    static void BuildGrassTufts()
    {
        var vertices=new System.Collections.Generic.List<Vector3>();var uv=new System.Collections.Generic.List<Vector2>();var colors=new System.Collections.Generic.List<Color>();var triangles=new System.Collections.Generic.List<int>();
        var rng=new System.Random(73491);int placed=0,tries=0;const int target=1450;
        while(placed<target&&tries<target*5)
        {
            tries++;float x=(float)(rng.NextDouble()*320-160),z=(float)(rng.NextDouble()*310-155);
            if(!IsOpenGrassSpot(x,z))continue;placed++;
            int blades=3+(placed%3);Color tint=Color.Lerp(new Color(.48f,.76f,.28f),new Color(.82f,.91f,.43f),(float)rng.NextDouble()*.75f);
            for(int blade=0;blade<blades;blade++)
            {
                float angle=(float)rng.NextDouble()*Mathf.PI*2f,width=.13f+(float)rng.NextDouble()*.18f,height=.34f+(float)rng.NextDouble()*.52f;
                Vector3 right=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(width*.5f);
                Vector3 basePoint=new Vector3(x,.018f,z),tip=basePoint+Vector3.up*height+new Vector3(Mathf.Sin(angle)*.12f,0,Mathf.Cos(angle)*.12f);
                int a=vertices.Count;vertices.Add(basePoint-right);vertices.Add(basePoint+right);vertices.Add(tip-right*.08f);vertices.Add(tip+right*.08f);
                uv.Add(new Vector2(0,0));uv.Add(new Vector2(1,0));uv.Add(new Vector2(0,1));uv.Add(new Vector2(1,1));
                colors.Add(tint*.77f);colors.Add(tint*.82f);colors.Add(tint*1.08f);colors.Add(tint*1.12f);
                triangles.Add(a);triangles.Add(a+2);triangles.Add(a+1);triangles.Add(a+1);triangles.Add(a+2);triangles.Add(a+3);
                triangles.Add(a+1);triangles.Add(a+2);triangles.Add(a);triangles.Add(a+3);triangles.Add(a+2);triangles.Add(a+1);
            }
        }
        var mesh=new Mesh();mesh.name="Hand-painted wind-swept grass tufts";mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        var clumps=new GameObject("Detailed moving grass tufts");Register(clumps);clumps.AddComponent<MeshFilter>().sharedMesh=mesh;clumps.AddComponent<MeshRenderer>().sharedMaterial=grassTufts;
    }
    static bool IsOpenGrassSpot(float x,float z)
    {
        if(InClanLot(x,z)||IsReservedUrbanSpot(x,z)||IsReservedNinjaSpot(x,z))return false;
        if(Mathf.Abs(z-RiverCenter(x))<20f)return false;
        if(Mathf.Abs(x)<11f&&z>-145&&z<88)return false;
        if(Mathf.Abs(z+165)<14f||Mathf.Abs(x-54)<8f||Mathf.Abs(x+55)<8f||Mathf.Abs(x-220)<10f||Mathf.Abs(x+220)<10f)return false;
        for(int i=0;i<housePositions.Count;i++)if((housePositions[i]-new Vector2(x,z)).sqrMagnitude<8.2f*8.2f)return false;
        return true;
    }
    static void Wall(string n,float x,float z,float w,float d)=>Cube(n,new Vector3(x,7,z),new Vector3(w,14,d),stone);
    static void WallWatchtower(string name,float x,float z)
    {
        Cylinder(name+" stone shaft",new Vector3(x,14,z),new Vector3(9,28,9),stone);
        Cylinder(name+" vermilion balcony",new Vector3(x,27.8f,z),new Vector3(14,1.8f,14),red);
        Cylinder(name+" tiled roof",new Vector3(x,32.2f,z),new Vector3(17,3,17),hokageRoof);
        Cylinder(name+" roof cap",new Vector3(x,34.5f,z),new Vector3(4,1.5f,4),wood);
    }
    static void Gate(string n,float x,float z,float rot)
    {
        var g=new GameObject(n);Register(g);g.transform.position=new Vector3(x,0,z);g.transform.rotation=Quaternion.Euler(0,rot,0);
        LocalCube(g,"Gate left pillar",new Vector3(-7,5,0),new Vector3(3,10,5),red);
        LocalCube(g,"Gate right pillar",new Vector3(7,5,0),new Vector3(3,10,5),red);
        LocalCube(g,"Gate lintel",new Vector3(0,10,0),new Vector3(20,2,6),red);
        LocalCube(g,"Gate upper beam",new Vector3(0,8.8f,0),new Vector3(15,.7f,6.5f),wood);
        LocalCube(g,"Gate roof cap",new Vector3(0,11.6f,0),new Vector3(23,.7f,8),roof);
        var name=new GameObject("Gate name");name.transform.SetParent(g.transform,false);name.transform.localPosition=new Vector3(0,8.8f,-3.3f);name.transform.localRotation=Quaternion.Euler(0,180,0);name.transform.localScale=new Vector3(-1,1,1);var t=name.AddComponent<TextMesh>();t.text="KONOHA";t.fontSize=50;t.characterSize=.16f;t.anchor=TextAnchor.MiddleCenter;t.color=Color.white;
    }
    static void House(float x,float z,int style,string label=null,float size=1)
    {
        int seed=Mathf.Abs(Mathf.RoundToInt(x*71+z*131+style*197));int variant=seed%5;
        float width=8.2f+(seed%27)*.1f,depth=7.8f+((seed/3)%24)*.1f,height=3.8f+(seed%8)*.85f;
        Material[] walls={plaster,sage,blush,ochre};Material[] roofs={roof,blueRoof,red,hokageRoof,wood};
        var root=new GameObject(label??"Konoha home");Register(root);root.transform.position=new Vector3(x,0,z);root.transform.localScale=Vector3.one*size;root.transform.rotation=Quaternion.Euler(0,(seed/7%4)*90,0);
        LocalCube(root,"Raised stone foundation",new Vector3(0,.2f,0),new Vector3(width+.7f,.5f,depth+.7f),stone);
        LocalCube(root,"Painted house walls",new Vector3(0,height/2+.35f,0),new Vector3(width,height,depth),walls[(seed/5)%walls.Length]);
        var roofMat=roofs[(seed/13)%roofs.Length];float eaveY=height+.7f;
        if(variant==0)
        {
            var left=LocalCube(root,"Gabled roof left wing",new Vector3(-width*.23f,eaveY,0),new Vector3(width*.62f,1.05f,depth+2),roofMat);left.transform.localRotation=Quaternion.Euler(0,0,-25);
            var right=LocalCube(root,"Gabled roof right wing",new Vector3(width*.23f,eaveY,0),new Vector3(width*.62f,1.05f,depth+2),roofMat);right.transform.localRotation=Quaternion.Euler(0,0,25);
            LocalCube(root,"Roof ridge",new Vector3(0,eaveY+1.1f,0),new Vector3(width+.8f,.55f,1),wood);
        }
        else if(variant==1)
        {
            for(int tier=0;tier<3;tier++){float tierScale=1f-tier*.2f;var roofTier=GameObject.CreatePrimitive(PrimitiveType.Cylinder);roofTier.name="Pagoda roof tier";roofTier.transform.SetParent(root.transform,false);roofTier.transform.localPosition=new Vector3(0,eaveY+tier*.7f,0);roofTier.transform.localScale=new Vector3((width+3)*tierScale,.45f,(depth+3)*tierScale);roofTier.GetComponent<Renderer>().sharedMaterial=roofMat;}
        }
        else if(variant==2)
        {
            var cap=GameObject.CreatePrimitive(PrimitiveType.Sphere);cap.name="Round tiled roof";cap.transform.SetParent(root.transform,false);cap.transform.localPosition=new Vector3(0,eaveY+.35f,0);cap.transform.localScale=new Vector3(width+3,2.2f,depth+3);cap.GetComponent<Renderer>().sharedMaterial=roofMat;
            LocalCube(root,"Round roof eave",new Vector3(0,eaveY-.15f,0),new Vector3(width+4,.35f,depth+4),wood);
        }
        else if(variant==3)
        {
            LocalCube(root,"Hipped roof base",new Vector3(0,eaveY,0),new Vector3(width+3,1.3f,depth+3),roofMat);
            LocalCube(root,"Raised roof tier",new Vector3(0,eaveY+1,0),new Vector3(width*.72f,.8f,depth*.72f),roofMat);
            LocalCube(root,"Roof finial",new Vector3(0,eaveY+1.9f,0),new Vector3(.8f,1.4f,.8f),wood);
        }
        else
        {
            LocalCube(root,"Flat tiled roof",new Vector3(0,eaveY,0),new Vector3(width+2,1,depth+2),roofMat);
            LocalCube(root,"Front awning",new Vector3(0,height*.72f,-depth*.62f),new Vector3(width*.78f,.35f,2.6f),red);
            LocalCube(root,"Roof chimney",new Vector3(width*.28f,eaveY+1.2f,depth*.2f),new Vector3(1.2f,2.4f,1.2f),stone);
        }
        LocalCube(root,"Main door",new Vector3(0,1.45f,-depth/2-.1f),new Vector3(1.7f,2.8f,.18f),wood);
        int windowCount=2+(seed/17)%3;int floors=Mathf.Max(1,Mathf.FloorToInt(height/2.8f));
        for(int floor=0;floor<floors;floor++)for(int i=0;i<windowCount;i++){float wx=(i-(windowCount-1)*.5f)*(width/(windowCount));float wy=1.9f+floor*2.5f;if(wy>height-.25f)continue;LocalCube(root,"Paper window",new Vector3(wx,wy,-depth/2-.12f),new Vector3(1.35f,1.45f,.14f),water);LocalCube(root,"Window divider",new Vector3(wx,wy,-depth/2-.22f),new Vector3(.12f,1.55f,.1f),wood);}
        if((seed%3)!=0)LocalCube(root,"Side annex",new Vector3(width*.55f,1.6f,depth*.15f),new Vector3(width*.35f,3.2f,depth*.55f),walls[(seed/5+1)%walls.Length]);
        if(variant==4){for(int i=-1;i<=1;i+=2)LocalCube(root,"Entry veranda post",new Vector3(i*width*.42f,1.8f,-depth*.62f),new Vector3(.25f,3.6f,.25f),wood);}
        string[] signs={"DANGO","TEA","TOOLS","BOOKS","FLOWERS","KONOHA","NINJA"};string signText=label??signs[(seed/23)%signs.Length];
        LocalCube(root,"Shop sign board",new Vector3(0,height+.1f,-depth/2-.3f),new Vector3(Mathf.Min(4.2f,width*.72f),.8f,.2f),wood);
        var nameplate=new GameObject("Shopfront lettering");nameplate.transform.SetParent(root.transform,false);nameplate.transform.localPosition=new Vector3(0,height+.1f,-depth/2-.43f);nameplate.transform.localRotation=Quaternion.Euler(0,180,0);nameplate.transform.localScale=new Vector3(-1,1,1);var tm=nameplate.AddComponent<TextMesh>();tm.text=signText;tm.fontSize=48;tm.characterSize=.11f;tm.anchor=TextAnchor.MiddleCenter;tm.color=new Color(1,.86f,.52f);
    }
    static void Tree(string n,Vector3 p,float scale=1)
    {
        var tree=new GameObject(n);Register(tree);tree.transform.position=p;tree.AddComponent<KonohaTreeSway>();
        var trunk=GameObject.CreatePrimitive(PrimitiveType.Cylinder);trunk.name=n+" trunk";trunk.transform.SetParent(tree.transform,false);trunk.transform.localPosition=Vector3.up*(2*scale);trunk.transform.localScale=new Vector3(.8f,2,.8f)*scale;trunk.GetComponent<Renderer>().sharedMaterial=wood;
        var crown=GameObject.CreatePrimitive(PrimitiveType.Sphere);crown.name=n+" canopy";crown.transform.SetParent(tree.transform,false);crown.transform.localPosition=Vector3.up*(5*scale);crown.transform.localScale=new Vector3(5,5,5)*scale;crown.GetComponent<Renderer>().sharedMaterial=leaf;
        for(int i=0;i<3;i++)
        {
            float a=i*Mathf.PI*2/3f+.6f;var cluster=GameObject.CreatePrimitive(PrimitiveType.Sphere);cluster.name=n+" foliage cluster";cluster.transform.SetParent(tree.transform,false);
            cluster.transform.localPosition=new Vector3(Mathf.Sin(a)*1.4f,5.5f+(i%2)*.55f,Mathf.Cos(a)*1.4f)*scale;
            cluster.transform.localScale=new Vector3(3.2f,2.8f,3.1f)*scale;cluster.GetComponent<Renderer>().sharedMaterial=leaf;
        }
    }
    static void Lantern(float x,float z){Cube("Lantern post",new Vector3(x,2,z),new Vector3(.25f,4,.25f),wood);Cube("Lantern",new Vector3(x,4,z),new Vector3(1.1f,1.1f,1.1f),red);}
    static void Pond(){var p=GameObject.CreatePrimitive(PrimitiveType.Cylinder);p.name="Memorial garden pond";p.transform.position=new Vector3(75,.08f,70);p.transform.localScale=new Vector3(20,.15f,15);p.GetComponent<Renderer>().sharedMaterial=riverWater;Register(p);for(int i=0;i<6;i++)Tree("Garden tree",new Vector3(75+Mathf.Cos(i*Mathf.PI/3)*16,0,70+Mathf.Sin(i*Mathf.PI/3)*13));}
    static void HokageOffice()
    {
        float x=0f,z=80f;
        Cylinder("Hokage residence courtyard",new Vector3(x,.14f,z-4),new Vector3(108,.28f,108),ochre);
        // Low golden enclosure with an open entrance on the south side.
        const int wallSegments=18;float yardRadius=56f;
        for(int i=0;i<wallSegments;i++)
        {
            if(i>=8&&i<=10)continue;
            float a=(i+.5f)*Mathf.PI*2/wallSegments;
            var part=Cube("Hokage courtyard ochre wall",new Vector3(Mathf.Sin(a)*yardRadius,1.25f,z-4+Mathf.Cos(a)*yardRadius),new Vector3(19.5f,2.5f,1.8f),hokageRoof);
            part.transform.rotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);
        }
        for(int side=-1;side<=1;side+=2)
        {
            Cylinder("Courtyard gate stone pier",new Vector3(side*8,2.4f,z-59),new Vector3(2.4f,4.8f,2.4f),sandstone);
            Cylinder("Courtyard gate gold cap",new Vector3(side*8,5.1f,z-59),new Vector3(3.1f,.6f,3.1f),hokageRoof);
        }
        // Broad, stacked red drum halls with projecting orange-gold eaves.
        Cylinder("Hokage office lower cylindrical hall",new Vector3(x,13,z),new Vector3(48,26,48),hokageRed);
        Cylinder("Hokage office lower gold eave",new Vector3(x,27,z),new Vector3(60,3.5f,60),hokageRoof);
        Cylinder("Hokage office middle cylindrical hall",new Vector3(x,34,z),new Vector3(40,12,40),hokageRed);
        Cylinder("Hokage office broad middle eave",new Vector3(x,41,z),new Vector3(64,3.8f,64),hokageRoof);
        Cylinder("Hokage office upper cylindrical hall",new Vector3(x,50,z),new Vector3(31,15,31),hokageRed);
        Cylinder("Hokage office upper roof rim",new Vector3(x,58,z),new Vector3(47,3.4f,47),hokageRoof);
        Cylinder("Hokage office dome drum",new Vector3(x,60,z),new Vector3(32,4,32),hokageRed);
        Cylinder("Hokage office dome golden lip",new Vector3(x,62,z),new Vector3(35,1.2f,35),hokageRoof);
        var dome=GameObject.CreatePrimitive(PrimitiveType.Sphere);dome.name="Hokage residence pale jade dome";dome.transform.position=new Vector3(x,69,z);dome.transform.localScale=new Vector3(31,15,31);dome.GetComponent<Renderer>().sharedMaterial=hokageDome;Register(dome);
        // Pale ribs and glazed slits make the cap read as a segmented anime dome.
        for(int i=0;i<12;i++)
        {
            float a=i*Mathf.PI*2/12f;float px=Mathf.Sin(a)*9.2f,pz=z+Mathf.Cos(a)*9.2f;
            var rib=Cube("Hokage dome ivory rib",new Vector3(px,69,pz),new Vector3(1.05f,10,.8f),plaster);rib.transform.rotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,Mathf.Sin(a)*-15f);
            var pane=Cube("Hokage dome glass slit",new Vector3(Mathf.Sin(a)*14,69.5f,z+Mathf.Cos(a)*14),new Vector3(2.2f,7,.55f),hokageGlass);pane.transform.rotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);
        }
        Cylinder("Hokage dome top finial",new Vector3(x,77,z),new Vector3(1.2f,2.2f,1.2f),sandstone);
        RadialWindows(x,z,10,23.7f,16,2.3f,4.2f);RadialWindows(x,z,34,19.8f,14,2.1f,3.2f);RadialWindows(x,z,50,15.6f,12,1.8f,3.6f);
        // Two lower red wings flank the main drum, like the reference frame.
        for(int side=-1;side<=1;side+=2)
        {
            float wx=side*34f,wz=z+3;
            Cylinder("Hokage residence side wing lower hall",new Vector3(wx,10,wz),new Vector3(29,20,29),hokageRed);
            Cylinder("Hokage residence side wing eave",new Vector3(wx,21,wz),new Vector3(37,3.2f,37),hokageRoof);
            Cylinder("Hokage residence side wing upper hall",new Vector3(wx,29,wz),new Vector3(23,12,23),hokageRed);
            Cylinder("Hokage residence side wing roof",new Vector3(wx,36,wz),new Vector3(32,3.2f,32),hokageRoof);
            RadialWindows(wx,wz,11,14.4f,10,1.6f,3f);
        }
        // Main entrance and circular insignia face the village approach (negative Z).
        Cube("Hokage office entrance shadow",new Vector3(x,4,z-24.5f),new Vector3(9,8,.65f),dark);
        Cube("Hokage office main doors",new Vector3(x,4,z-25f),new Vector3(6,7,.5f),wood);
        for(int side=-1;side<=1;side+=2)Cylinder("Hokage office entrance pillar",new Vector3(x+side*7,5,z-24),new Vector3(1.5f,10,1.5f),plaster);
        Cylinder("Hokage office entrance canopy",new Vector3(x,9,z-25),new Vector3(18,1.5f,8),hokageRoof);
        Cylinder("Hokage office round front emblem",new Vector3(x,51,z-16.4f),new Vector3(7.2f,7.2f,1.2f),hokageRoof);
        var emblem=new GameObject("Fire emblem on Hokage office");Register(emblem);emblem.transform.position=new Vector3(x,51,z-17.1f);emblem.transform.rotation=Quaternion.Euler(0,180,0);emblem.transform.localScale=new Vector3(-1,1,1);var kanji=emblem.AddComponent<TextMesh>();kanji.text="火";kanji.fontSize=100;kanji.characterSize=.105f;kanji.anchor=TextAnchor.MiddleCenter;kanji.color=new Color(1,.9f,.55f);
    }
    static void Cylinder(string n,Vector3 p,Vector3 scale,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.position=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;Register(g);}
    static void RadialWindows(float x,float z,float y,float radius,int count,float w,float h)
    {
        for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count;var p=new Vector3(x+Mathf.Sin(a)*radius,y,z+Mathf.Cos(a)*radius);var window=Cube("Hokage office radial window",p,new Vector3(w,h,.2f),water);window.transform.rotation=Quaternion.Euler(0,i*360f/count,0);}
    }
    static void MarketStalls()
    {
        for(int row=0;row<2;row++)for(int i=0;i<7;i++)
        {
            float x=-30+i*10,z=row==0?-146f:-184f;
            Cube("Market stall counter",new Vector3(x,1,z),new Vector3(7,2,4),wood);
            var awning=Cube("Market stall canvas",new Vector3(x,4,z),new Vector3(8,.5f,5),i%2==0?red:plaster);
            for(int side=-1;side<=1;side+=2)Cube("Market stall post",new Vector3(x+side*3.4f,3,z),new Vector3(.35f,5,.35f),wood);
            for(int fruit=0;fruit<3;fruit++){var orb=GameObject.CreatePrimitive(PrimitiveType.Sphere);orb.name="Market produce";orb.transform.position=new Vector3(x-2+fruit*2,2.3f,z-.5f);orb.transform.localScale=Vector3.one*.8f;orb.GetComponent<Renderer>().sharedMaterial=fruit==1?red:leaf;Register(orb);}
        }
    }
    static void CreateUrbanDistricts()
    {
        // The main market sits south of the Hokage compound, as in the overhead reference.
        Cube("Commercial district market street",new Vector3(0,.015f,-165),new Vector3(520,.13f,20),road);
        Cylinder("Commercial district fountain",new Vector3(0,.35f,-165),new Vector3(15,.7f,15),sandstone);
        DistrictSign("Commercial district",0,-193,"COMMERCIAL",wood);

        // Hospital on the Hokage compound's east side.
        float hx=76f,hz=28f;
        Cube("Konoha hospital grounds",new Vector3(hx,.08f,hz),new Vector3(48,.16f,42),plaster);
        ClanBuilding("Konoha hospital central ward",hx,hz+5,28,23,13,plaster,blueRoof,red);
        ClanBuilding("Konoha hospital west wing",hx-18,hz-12,14,13,8,plaster,blueRoof,red);
        ClanBuilding("Konoha hospital east wing",hx+18,hz-12,14,13,8,plaster,blueRoof,red);
        Cube("Hospital red cross vertical",new Vector3(hx,12,hz-7),new Vector3(2,7,.7f),red);
        Cube("Hospital red cross horizontal",new Vector3(hx,12,hz-7),new Vector3(7,2,.8f),red);
        DistrictSign("Konoha hospital",hx,hz-20,"HOSPITAL",red);

        // Large open arena west of the central residential blocks.
        float ax=-152f,az=-105f;
        Cylinder("Konoha arena outer stone stand",new Vector3(ax,.6f,az),new Vector3(150,1.2f,150),stone);
        Cylinder("Konoha arena packed sand field",new Vector3(ax,1.24f,az),new Vector3(108,.12f,108),ochre);
        for(int i=0;i<16;i++)
        {
            float a=i*Mathf.PI*2/16f;var bench=Cube("Konoha arena spectator stand",new Vector3(ax+Mathf.Sin(a)*59,3.1f,az+Mathf.Cos(a)*59),new Vector3(24,4,8),sandstone);
            bench.transform.rotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);
        }
        DistrictSign("Konoha arena sign",ax,az-44,"ARENA",red);

        // A compact workshop quarter occupies the southeast district.
        float ix=190f,iz=-205f;
        Cube("Industrial district paved yard",new Vector3(ix,.06f,iz),new Vector3(94,.12f,74),road);
        for(int i=0;i<4;i++)
        {
            float bx=ix-30+(i%2)*58,bz=iz-20+(i/2)*39;
            ClanBuilding("Konoha workshop " + (i+1),bx,bz,22,17,8+i%2*2,stone,blueRoof,red);
            Cylinder("Workshop chimney",new Vector3(bx+7,13+i%2*2,bz+5),new Vector3(2.8f,12,2.8f),dark);
        }
        DistrictSign("Industrial district sign",ix,iz-42,"INDUSTRIAL",wood);
        DistrictSign("Western residential district sign",-150,-228,"RESIDENTIAL WEST",wood);
        DistrictSign("Eastern residential district sign",140,-228,"RESIDENTIAL EAST",wood);
    }
    static void DistrictSign(string name,float x,float z,string label,Material signMaterial)
    {
        Cube(name+" timber signboard",new Vector3(x,5,z),new Vector3(19,4.4f,.7f),signMaterial);
        var sign=new GameObject(name+" lettering");Register(sign);sign.transform.position=new Vector3(x,5,z-.42f);sign.transform.rotation=Quaternion.Euler(0,180,0);sign.transform.localScale=new Vector3(-1,1,1);
        var text=sign.AddComponent<TextMesh>();text.text=label;text.fontSize=48;text.characterSize=.11f;text.anchor=TextAnchor.MiddleCenter;text.color=Color.white;
    }
    static void CreateNinjaInfrastructure()
    {
        NinjaFacility("ANBU operations compound",-238,-104,42,38,dark,blueRoof,plaster,0);
        NinjaFacility("Konoha police headquarters",240,-30,40,38,stone,red,plaster,1);
        NinjaFacility("Mission administration center",101,-95,44,38,ochre,hokageRoof,red,2);
        NinjaFacility("Shinobi barracks",-245,120,42,38,plaster,blueRoof,wood,3);
    }
    static void NinjaFacility(string name,float x,float z,float width,float depth,Material wallMat,Material roofMat,Material accent,int style)
    {
        var parent=activeLayerRoot;var site=new GameObject(name).transform;Register(site.gameObject);activeLayerRoot=site;
        Cube(name+" courtyard",new Vector3(x,.08f,z),new Vector3(width,.16f,depth),style==0?dark:road);
        Cube(name+" west fence",new Vector3(x-width*.5f,1.7f,z),new Vector3(1.2f,3.4f,depth),wallMat);
        Cube(name+" east fence",new Vector3(x+width*.5f,1.7f,z),new Vector3(1.2f,3.4f,depth),wallMat);
        Cube(name+" north fence",new Vector3(x,1.7f,z+depth*.5f),new Vector3(width,3.4f,1.2f),wallMat);
        Cube(name+" south fence left",new Vector3(x-width*.32f,1.7f,z-depth*.5f),new Vector3(width*.36f,3.4f,1.2f),wallMat);
        Cube(name+" south fence right",new Vector3(x+width*.32f,1.7f,z-depth*.5f),new Vector3(width*.36f,3.4f,1.2f),wallMat);
        ClanBuilding(name+" main building",x,z+3,width*.58f,depth*.48f,style==2?12:10,wallMat,roofMat,accent);
        Cube(name+" entry beam",new Vector3(x,5,z-depth*.5f),new Vector3(width*.35f,1.6f,2.4f),accent);
        string[] signLabels={"ANBU","POLICÍA","MISIONES","BARRACAS"};
        DistrictSign(name+" sign",x,z-depth*.5f-5,signLabels[style],accent);
        if(style==0)
        {
            for(int side=-1;side<=1;side+=2)Cylinder("ANBU mask display plinth",new Vector3(x+side*width*.34f,.6f,z+depth*.2f),new Vector3(3,1.2f,3),stone);
            for(int i=-1;i<=1;i++)Cube("ANBU weapons rack",new Vector3(x+i*8,2,z-depth*.34f),new Vector3(4,4,.7f),wood);
        }
        else if(style==1)
        {
            Cube("Police crest plaque",new Vector3(x,9,z-depth*.5f-.8f),new Vector3(8,5,.8f),red);
            for(int side=-1;side<=1;side+=2)Cube("Police lantern post",new Vector3(x+side*width*.35f,3,z-depth*.2f),new Vector3(.7f,6,.7f),wood);
        }
        else if(style==2)
        {
            for(int i=-1;i<=1;i++)
            {
                Cube("Mission scroll notice board",new Vector3(x+i*11,4,z-depth*.31f),new Vector3(7,5,.8f),wood);
                Cube("Mission parchment",new Vector3(x+i*11,4,z-depth*.31f-.48f),new Vector3(5.2f,3.3f,.12f),plaster);
            }
        }
        else
        {
            for(int side=-1;side<=1;side+=2)ClanBuilding("Barracks sleeping wing",x+side*17f,z+2,8,depth*.5f,6,plaster,blueRoof,wood);
            Cube("Barracks drill square",new Vector3(x,.1f,z-depth*.27f),new Vector3(width*.62f,.2f,8),sandstone);
        }
        activeLayerRoot=parent;
    }
    static void CreateOutskirts()
    {
        Cube("South gate road to frontier",new Vector3(0,.02f,-374),new Vector3(14,.14f,192),road);
        Cube("West gate road into forest",new Vector3(-350,.02f,0),new Vector3(145,.14f,13),road);
        Cube("East gate road to waterfall trail",new Vector3(350,.02f,0),new Vector3(145,.14f,13),road);
        // Small agricultural plots sit outside the south wall beside the training field.
        Cube("Konoha south farm plots",new Vector3(145,.07f,-365),new Vector3(82,.14f,62),ochre);
        for(int i=-4;i<=4;i++)
        {
            Cube("Farm crop row",new Vector3(145+i*8,.22f,-365),new Vector3(1.5f,.28f,48),i%2==0?leaf:sage);
            for(int j=-2;j<=2;j++)Sphere("Farm crop cluster",new Vector3(145+i*8,.65f,-365+j*9),new Vector3(2.3f,1.1f,2.5f),i%2==0?leaf:sage);
        }
        for(int side=-1;side<=1;side+=2)
        {
            Cube("Farm timber fence",new Vector3(145+side*43,1,-365),new Vector3(1,2,64),wood);
            Cube("Farm timber fence",new Vector3(145,1,-365+side*33),new Vector3(86,2,1),wood);
        }
        ClanBuilding("Farmers storehouse",203,-333,17,14,6,plaster,roof,wood);
        WallWatchtower("South frontier watchpost",-48,-436);
        WallWatchtower("Western frontier watchpost",-433,-38);
        DistrictSign("Konoha frontier",25,-405,"TRAINING & FARMLAND",wood);
    }
    static void CreateNpcCrowd()
    {
        Vector3[] route={new Vector3(-60,0,-150),new Vector3(60,0,-150),new Vector3(70,0,-70),new Vector3(0,0,8),new Vector3(-70,0,-70)};
        for(int p=0;p<route.Length;p++)route[p]=activeLayerRoot.parent.TransformPoint(route[p]);
        for(int i=0;i<20;i++)
        {
            var npc=GameObject.CreatePrimitive(PrimitiveType.Capsule);npc.name="Konoha villager patrol "+(i+1);Register(npc);
            int lane=i%route.Length;int lap=i/route.Length;
            npc.transform.position=route[lane]+new Vector3((lap-2)*7,1,(i%2)*5);
            npc.GetComponent<Renderer>().sharedMaterial=i%3==0?red:(i%3==1?plaster:sage);
            npc.AddComponent<KonohaNpcPatrol>().SetRoute(route,lane);
        }
    }
    static void PopulateResidentialBlocks()
    {
        var rng=new System.Random(140219);housePositions.Clear();var occupied=housePositions;
        for(int row=0;row<27;row++)for(int col=0;col<39;col++)
        {
            float baseX=-250+col*13.5f+(row%2==0?0:6.75f),baseZ=-240+row*13.5f;
            float x=baseX+(float)(rng.NextDouble()*10-5),z=baseZ+(float)(rng.NextDouble()*10-5);
            if(x < -255 || x > 255 || z < -250 || z > 110 || rng.NextDouble()<.16)continue;
            if(Mathf.Abs(x)<15 || Mathf.Abs(x-54)<10 || Mathf.Abs(x+55)<10 || Mathf.Abs(x-220)<10 || Mathf.Abs(x+220)<10 || Mathf.Abs(z)<13)continue;
            if(Mathf.Abs(z+165)<14)continue;
            if(Mathf.Abs(z-RiverCenter(x))<19)continue;
            if(z>104)continue;
            if(IsReservedUrbanSpot(x,z)||IsReservedNinjaSpot(x,z)||InClanLot(x,z))continue;
            bool crowded=false;Vector2 candidate=new Vector2(x,z);
            for(int i=0;i<occupied.Count;i++)if((occupied[i]-candidate).sqrMagnitude<11.8f*11.8f){crowded=true;break;}
            if(crowded)continue;
            occupied.Add(candidate);
            float size=.78f+(float)rng.NextDouble()*.44f;
            House(x,z,rng.Next(3),null,size);
        }
    }
    static bool IsReservedUrbanSpot(float x,float z)
    {
        if(new Vector2(x,z-80f).sqrMagnitude<61f*61f)return true;
        if(Mathf.Abs(x-76f)<32f&&Mathf.Abs(z-28f)<29f)return true;
        if((new Vector2(x,z)-new Vector2(-152f,-105f)).sqrMagnitude<51f*51f)return true;
        if(Mathf.Abs(x)<43f&&Mathf.Abs(z+165f)<15f)return true;
        if(Mathf.Abs(x)<42f&&(Mathf.Abs(z+146f)<8f||Mathf.Abs(z+184f)<8f))return true;
        if(Mathf.Abs(x-150f)<21f&&Mathf.Abs(z+228f)<7f||Mathf.Abs(x+150f)<21f&&Mathf.Abs(z+228f)<7f)return true;
        if(Mathf.Abs(x-190f)<59f&&Mathf.Abs(z+205f)<49f)return true;
        if(Mathf.Abs(x+35f)<25f&&Mathf.Abs(z-5f)<17f)return true;
        if(Mathf.Abs(x-34f)<18f&&Mathf.Abs(z-19f)<17f)return true;
        if(Mathf.Abs(x-75f)<24f&&Mathf.Abs(z-70f)<22f)return true;
        return false;
    }
    static bool IsReservedNinjaSpot(float x,float z)
    {
        return (Mathf.Abs(x+238f)<29f&&Mathf.Abs(z+104f)<27f)
            ||(Mathf.Abs(x-240f)<27f&&Mathf.Abs(z+30f)<25f)
            ||(Mathf.Abs(x-101f)<29f&&Mathf.Abs(z+95f)<26f)
            ||(Mathf.Abs(x+245f)<27f&&Mathf.Abs(z-120f)<29f);
    }
    static bool InClanLot(float x,float z)
    {
        Vector2 point=new Vector2(x,z);for(int i=0;i<clanDistrictCenters.Length;i++)if((point-clanDistrictCenters[i]).sqrMagnitude<48f*48f)return true;return false;
    }
    static void CreateClanDistricts()
    {
        ClanCompound("Inuzuka","PAW",-205,180,ochre,roof,red,0);
        ClanCompound("Nara","DEER",-68,184,wood,blueRoof,sage,1);
        ClanCompound("Aburame","INSECT",70,184,stone,blueRoof,dark,2);
        ClanCompound("Hyuga","BYAKUGAN",207,180,plaster,blueRoof,hokageRoof,3);
        ClanCompound("Yamanaka","FLOWER",-205,56,blush,roof,ochre,4);
        ClanCompound("Sarutobi","FIRE",-90,55,red,hokageRoof,plaster,5);
        ClanCompound("Uchiha","FAN",205,56,dark,blueRoof,red,6);
        ClanCompound("Akimichi","SPIRAL",-205,-190,ochre,roof,leaf,7);
    }
    static void ClanCompound(string name,string emblem,float x,float z,Material wallMat,Material roofMat,Material accent,int style)
    {
        var parent=activeLayerRoot;var compound=new GameObject("Clan - "+name).transform;Register(compound.gameObject);activeLayerRoot=compound;
        Cube(name+" walled grounds",new Vector3(x,.07f,z),new Vector3(70,.14f,70),style==4||style==7?ochre:road);
        Cube(name+" west courtyard wall",new Vector3(x-35,2,z),new Vector3(1.5f,4,70),wallMat);
        Cube(name+" east courtyard wall",new Vector3(x+35,2,z),new Vector3(1.5f,4,70),wallMat);
        Cube(name+" north courtyard wall",new Vector3(x,2,z+35),new Vector3(70,4,1.5f),wallMat);
        Cube(name+" south wall left",new Vector3(x-25,2,z-35),new Vector3(20,4,1.5f),wallMat);
        Cube(name+" south wall right",new Vector3(x+25,2,z-35),new Vector3(20,4,1.5f),wallMat);
        Cube(name+" gate left pillar",new Vector3(x-13,3.5f,z-35),new Vector3(3,7,3),accent);
        Cube(name+" gate right pillar",new Vector3(x+13,3.5f,z-35),new Vector3(3,7,3),accent);
        Cube(name+" clan gate beam",new Vector3(x,7,z-35),new Vector3(30,2,4),roofMat);
        ClanBuilding(name+" main dojo",x,z+11,26,20,10,wallMat,roofMat,accent);
        ClanBuilding(name+" west residence",x-19,z-13,15,14,6.5f,wallMat,roofMat,accent);
        ClanBuilding(name+" east residence",x+19,z-13,15,14,6.5f,wallMat,roofMat,accent);
        Cube(name+" crest plaque",new Vector3(x,9,z-36.1f),new Vector3(13,5,.6f),accent);
        var title=new GameObject(name+" clan name and emblem");Register(title);title.transform.position=new Vector3(x,9,z-36.5f);title.transform.rotation=Quaternion.Euler(0,180,0);title.transform.localScale=new Vector3(-1,1,1);
        var text=title.AddComponent<TextMesh>();text.text=name.ToUpper()+"\n"+emblem;text.fontSize=60;text.characterSize=.105f;text.anchor=TextAnchor.MiddleCenter;text.color=Color.white;
        Cylinder(name+" training circle",new Vector3(x,.17f,z-24),new Vector3(18,.22f,18),style==4||style==7?ochre:stone);
        if(style==0)
        {
            ClanBuilding("Inuzuka kennel north",x,z+27,15,9,4.5f,wood,roof,red);
            for(int i=0;i<5;i++)Sphere("Inuzuka scent trail marker",new Vector3(x-20+i*10,.55f,z-3),new Vector3(1.3f,1.1f,1.3f),accent);
        }
        else if(style==1)
        {
            float[] groveX={-26,26,-26,26,0},groveZ={-25,-25,24,24,-27};
            for(int i=0;i<groveX.Length;i++)Tree("Nara clan deer grove",new Vector3(x+groveX[i],0,z+groveZ[i]),.72f);
        }
        else if(style==2)
        {
            Cube("Aburame insect greenhouse",new Vector3(x,4,z+27),new Vector3(18,8,8),hokageGlass);
            for(int i=-2;i<=2;i++)Cylinder("Aburame garden hive",new Vector3(x+i*5,.7f,z-24),new Vector3(2.4f,1.4f,2.4f),dark);
        }
        else if(style==3)
        {
            for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Cylinder("Hyuga gentle fist target",new Vector3(x+Mathf.Cos(a)*21,.9f,z+Mathf.Sin(a)*21),new Vector3(2,1.8f,2),plaster);}
        }
        else if(style==4)
        {
            for(int i=0;i<13;i++)Sphere("Yamanaka flower planter",new Vector3(x-24+(i%5)*12,.8f,z-24+(i/5)*12),new Vector3(1.3f,1.5f,1.3f),i%2==0?blush:hokageRoof);
        }
        else if(style==5)
        {
            for(int side=-1;side<=1;side+=2)Cylinder("Sarutobi fire training brazier",new Vector3(x+side*21,2,z+22),new Vector3(2.4f,4,2.4f),hokageRoof);
        }
        else if(style==6)
        {
            Cube("Uchiha fan court marking",new Vector3(x, .22f,z-22),new Vector3(15,.16f,4),red);
            Cylinder("Uchiha fire crest",new Vector3(x,7,z-24),new Vector3(6,6,1.5f),red);
        }
        else
        {
            ClanBuilding("Akimichi family dining hall",x,z+28,22,10,7,wallMat,roofMat,accent);
            ClanBuilding("Akimichi ration store",x+22,z+10,9,8,5,wallMat,roofMat,accent);
            for(int side=-1;side<=1;side+=2)
            {
                Cube("Akimichi kitchen garden bed",new Vector3(x+side*23,.18f,z-23),new Vector3(8,.3f,5),sage);
                for(int i=0;i<3;i++)Sphere("Akimichi vegetables",new Vector3(x+side*23-2+i*2,.8f,z-23),new Vector3(1.2f,1.1f,1.2f),leaf);
            }
        }
        compound.localScale=new Vector3(1.38f,1f,1.38f);
        if(z>130)
        {
            float roadLength=z-35-138;
            if(roadLength>2)Cube(name+" north ward access lane",new Vector3(x,.015f,138+roadLength*.5f),new Vector3(7,.12f,roadLength),road);
        }
        activeLayerRoot=parent;
    }
    static void ClanBuilding(string name,float x,float z,float width,float depth,float height,Material wallMat,Material roofMat,Material accent)
    {
        Cube(name+" foundation",new Vector3(x,.25f,z),new Vector3(width+.8f,.5f,depth+.8f),stone);
        Cube(name+" plaster and timber walls",new Vector3(x,height*.5f+.5f,z),new Vector3(width,height,depth),wallMat);
        Cube(name+" projecting roof eaves",new Vector3(x,height+1,z),new Vector3(width+4,1.1f,depth+4),roofMat);
        Cube(name+" raised roof tier",new Vector3(x,height+2,z),new Vector3(width*.72f,1,depth*.72f),roofMat);
        Cube(name+" south entrance",new Vector3(x,1.7f,z-depth*.5f-.12f),new Vector3(2.4f,3.2f,.25f),accent);
        for(int side=-1;side<=1;side+=2)Cube(name+" paper window",new Vector3(x+side*width*.28f,2.8f,z-depth*.5f-.16f),new Vector3(2.1f,2.1f,.2f),water);
    }
    static float RiverCenter(float x)
    {
        float u=(x+160f)/320f;
        return -48f+Mathf.Sin(u*Mathf.PI*2f)*8f+Mathf.Sin(u*Mathf.PI*5f)*2.5f;
    }
    static Vector3 RiverSideVector(float x)
    {
        float xa=Mathf.Max(-500f,x-.5f),xb=Mathf.Min(500f,x+.5f);
        float slope=(RiverCenter(xb)-RiverCenter(xa))/Mathf.Max(.01f,xb-xa);
        return new Vector3(-slope,0,1).normalized;
    }
    static void RiverApproachLane(string name,float x,float width,float southEdge,float northEdge)
    {
        float center=RiverCenter(x),halfBridge=18f;float southEnd=center-halfBridge,northStart=center+halfBridge;
        float southLength=southEnd-southEdge,northLength=northEdge-northStart;
        if(southLength>0)Cube(name+" south bank approach",new Vector3(x,.02f,(southEdge+southEnd)*.5f),new Vector3(width,.14f,southLength),road);
        if(northLength>0)Cube(name+" north bank approach",new Vector3(x,.02f,(northStart+northEdge)*.5f),new Vector3(width,.14f,northLength),road);
    }
    static void VillageRiver()
    {
        RiverRibbon("Konoha riverbed",0f,15f,-2.4f,sandstone);
        RiverBankSlope("Konoha south riverbank slope",-1f);
        RiverBankSlope("Konoha north riverbank slope",1f);
        RiverRibbon("Konoha river water",0f,14f,-1.25f,riverWater);
        float[] crossings={-220f,-55f,0f,54f,220f};
        foreach(float x in crossings)
        {
            float z=RiverCenter(x);
            var deck=Cube("Sandstone village footbridge",new Vector3(x,.085f,z),new Vector3(20,.27f,36),sandstone);
            deck.transform.rotation=Quaternion.Euler(0,0,0);
            for(int side=-1;side<=1;side+=2)Cylinder("Bridge stone river abutment",new Vector3(x,-1.12f,z+side*16),new Vector3(3.3f,2.7f,3.3f),sandstone);
            for(int side=-1;side<=1;side+=2)
                for(int i=-1;i<=1;i++)Cube("Bridge timber rail",new Vector3(x+side*7.2f,.74f,z+i*8f),new Vector3(.45f,1.25f,.45f),wood);
            Cube("Bridge handrail",new Vector3(x-7.2f,1.3f,z),new Vector3(.4f,.28f,35),wood);
            Cube("Bridge handrail",new Vector3(x+7.2f,1.3f,z),new Vector3(.4f,.28f,35),wood);
        }
    }
    static void CreateEasternWaterfall()
    {
        float x=356f,z=RiverCenter(350f);
        Cube("Eastern cascade basalt cliff",new Vector3(x+9,14,z),new Vector3(18,32,64),stone);
        Cylinder("Eastern cascade upper pool",new Vector3(x-5,29.6f,z),new Vector3(38,.35f,40),riverWater);
        Cylinder("Eastern cascade plunge pool",new Vector3(x-8,-1.35f,z),new Vector3(42,.35f,46),riverWater);
        const float faceX=346f,width=38f,top=29.35f,bottom=-1.1f;
        var vertices=new[]{new Vector3(faceX,bottom,z-width*.5f),new Vector3(faceX,bottom,z+width*.5f),new Vector3(faceX,top,z-width*.5f),new Vector3(faceX,top,z+width*.5f)};
        var uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(7,0),new Vector2(7,1)};var mesh=new Mesh();mesh.name="Animated falling water sheet";
        mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=new[]{0,2,1,1,2,3};mesh.RecalculateNormals();mesh.RecalculateBounds();
        var sheet=new GameObject("Animated Eastern waterfall");Register(sheet);sheet.AddComponent<MeshFilter>().sharedMesh=mesh;sheet.AddComponent<MeshRenderer>().sharedMaterial=riverWater;
        for(int i=0;i<7;i++)
        {
            float a=i*Mathf.PI/3f;Sphere("Cascade basin boulder",new Vector3(x-8+Mathf.Sin(a)*24,-.5f,z+Mathf.Cos(a)*24),new Vector3(5,2.4f,4),sandstone);
        }
    }
    static void RiverBankSlope(string name,float sideSign)
    {
        const int segments=128;var vertices=new Vector3[(segments+1)*2];var uv=new Vector2[vertices.Length];var triangles=new int[segments*6];
        for(int i=0;i<=segments;i++)
        {
            float u=i/(float)segments,x=Mathf.Lerp(-500f,500f,u),z=RiverCenter(x);
            Vector3 side=RiverSideVector(x);Vector3 center=new Vector3(x,0,z);
            vertices[i*2]=center+side*(sideSign*16f)+Vector3.up*(-.05f);
            vertices[i*2+1]=center+side*(sideSign*7.6f)+Vector3.up*(-2.3f);
            uv[i*2]=new Vector2(u*18,0);uv[i*2+1]=new Vector2(u*18,1);
        }
        int t=0;for(int i=0;i<segments;i++){int a=i*2,b=a+1,c=a+2,d=a+3;if(sideSign>0){triangles[t++]=a;triangles[t++]=c;triangles[t++]=b;triangles[t++]=b;triangles[t++]=c;triangles[t++]=d;}else{triangles[t++]=a;triangles[t++]=b;triangles[t++]=c;triangles[t++]=b;triangles[t++]=d;triangles[t++]=c;}}
        var mesh=new Mesh();mesh.name=name+" mesh";mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
        var bank=new GameObject(name);Register(bank);bank.AddComponent<MeshFilter>().sharedMesh=mesh;bank.AddComponent<MeshRenderer>().sharedMaterial=sandstone;bank.AddComponent<MeshCollider>().sharedMesh=mesh;
    }
    static void RiverRibbon(string name,float lateralOffset,float width,float y,Material material)
    {
        const int segments=128;var vertices=new Vector3[(segments+1)*2];var uv=new Vector2[vertices.Length];var triangles=new int[segments*6];
        for(int i=0;i<=segments;i++)
        {
            float u=i/(float)segments;float x=-500f+u*1000f;float z=RiverCenter(x);
            Vector3 side=RiverSideVector(x);
            Vector3 center=new Vector3(x,y,z)+side*lateralOffset;
            vertices[i*2]=center-side*(width*.5f);vertices[i*2+1]=center+side*(width*.5f);
            uv[i*2]=new Vector2(u*26f,0);uv[i*2+1]=new Vector2(u*26f,1);
        }
        int t=0;for(int i=0;i<segments;i++){int a=i*2;triangles[t++]=a;triangles[t++]=a+1;triangles[t++]=a+2;triangles[t++]=a+1;triangles[t++]=a+3;triangles[t++]=a+2;}
        var mesh=new Mesh();mesh.name=name+" mesh";mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
        var river=new GameObject(name);Register(river);river.AddComponent<MeshFilter>().sharedMesh=mesh;river.AddComponent<MeshRenderer>().sharedMaterial=material;if(name.Contains("riverbed"))river.AddComponent<MeshCollider>().sharedMesh=mesh;
    }
    static void CreateHokageCliff()
    {
        const int nx=49,ny=19,frontCount=nx*ny;const float cliffWidth=560f,frontZ=230f,backZ=324f;
        var vertices=new Vector3[frontCount*2];var uv=new Vector2[frontCount*2];var frontTriangles=new System.Collections.Generic.List<int>();
        var backTriangles=new System.Collections.Generic.List<int>();var sideTriangles=new System.Collections.Generic.List<int>();
        float bottomV=hokageRockTexture==null?0f:.5f/hokageRockTexture.height;
        var heights=new float[frontCount];var topVs=new float[nx];
        for(int y=0;y<ny;y++)for(int x=0;x<nx;x++)
        {
            float u=(float)x/(nx-1),v=(float)y/(ny-1);
            int topPixel=0;
            if(hokageRockTexture!=null)
            {
                int sourceX=Mathf.Clamp(Mathf.RoundToInt(u*(hokageRockTexture.width-1)),0,hokageRockTexture.width-1);
                topPixel=hokageRockTexture.height-1;
                for(int py=0;py<hokageRockTexture.height;py++)
                {
                    Color sample=hokageRockTexture.GetPixel(sourceX,hokageRockTexture.height-1-py);
                    if(sample.r<.96f||sample.g<.96f||sample.b<.96f){topPixel=py;break;}
                }
            }
            float topV=hokageRockTexture==null?1f:1f-(topPixel+.5f)/hokageRockTexture.height;
            float height=hokageRockTexture==null?180f:cliffWidth*(hokageRockTexture.height-topPixel)/(float)hokageRockTexture.width;
            int index=y*nx+x;heights[index]=v*height;if(y==0)topVs[x]=topV;
            vertices[index]=new Vector3((u-.5f)*cliffWidth,v*height,frontZ+Mathf.Sin(x*.8f+y*1.3f)*.18f);
            uv[index]=new Vector2(u,Mathf.Lerp(bottomV,topV,v));
        }
        for(int y=0;y<ny;y++)for(int x=0;x<nx;x++)
        {
            int index=y*nx+x,back=index+frontCount;float u=(float)x/(nx-1);
            vertices[back]=new Vector3((u-.5f)*(cliffWidth*.94f),heights[index]*.83f,backZ);
            uv[back]=new Vector2(1f-u,Mathf.Lerp(bottomV,topVs[x],(float)y/(ny-1)));
        }
        for(int y=0;y<ny-1;y++)for(int x=0;x<nx-1;x++)
        {
            int a=y*nx+x,b=a+nx,c=a+1,d=b+1;
            frontTriangles.Add(a);frontTriangles.Add(b);frontTriangles.Add(c);frontTriangles.Add(c);frontTriangles.Add(b);frontTriangles.Add(d);
            int ba=a+frontCount,bb=b+frontCount,bc=c+frontCount,bd=d+frontCount;
            backTriangles.Add(ba);backTriangles.Add(bc);backTriangles.Add(bb);backTriangles.Add(bc);backTriangles.Add(bd);backTriangles.Add(bb);
        }
        var boundary=new System.Collections.Generic.List<int>();
        for(int y=0;y<ny;y++)boundary.Add(y*nx);
        for(int x=1;x<nx;x++)boundary.Add((ny-1)*nx+x);
        for(int y=ny-2;y>=0;y--)boundary.Add(y*nx+nx-1);
        for(int x=nx-2;x>=1;x--)boundary.Add(x);
        for(int i=0;i<boundary.Count;i++)
        {
            int a=boundary[i],b=boundary[(i+1)%boundary.Count],ba=a+frontCount,bb=b+frontCount;
            sideTriangles.Add(a);sideTriangles.Add(bb);sideTriangles.Add(b);
            sideTriangles.Add(a);sideTriangles.Add(ba);sideTriangles.Add(bb);
        }
        var mesh=new Mesh();mesh.name="Deep Hokage Rock with persistent image on both sides";mesh.vertices=vertices;mesh.uv=uv;mesh.subMeshCount=3;
        mesh.SetTriangles(frontTriangles,0);mesh.SetTriangles(sideTriangles,1);mesh.SetTriangles(backTriangles,2);mesh.RecalculateNormals();mesh.RecalculateBounds();
        var cliff=new GameObject("Hokage Rock - wide deep cliff with improved reference image");Register(cliff);cliff.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=cliff.AddComponent<MeshRenderer>();renderer.sharedMaterials=new[]{hokageRockReference,sandstone,hokageRockReference};cliff.AddComponent<MeshCollider>().sharedMesh=mesh;
    }
    static void CarvedHokageFace(int index)
    {
        string[] names={"Hashirama - First Hokage","Tobirama - Second Hokage","Hiruzen - Third Hokage","Minato - Fourth Hokage","Tsunade - Fifth Hokage","Kakashi - Sixth Hokage","Naruto - Seventh Hokage"};
        int[] positions={-150,-100,-50,0,50,100,150};float x=positions[index],y=65,z=147;
        Sphere(names[index]+" shoulders",new Vector3(x,y-24,z+2),new Vector3(49,31,12),sandstone);
        Sphere(names[index]+" face",new Vector3(x,y,z),new Vector3(39,43,13),sandstone);
        Sphere("Carved ear",new Vector3(x-21,y+1,z),new Vector3(7,12,8),sandstone);
        Sphere("Carved ear",new Vector3(x+21,y+1,z),new Vector3(7,12,8),sandstone);
        Sphere("Carved nose",new Vector3(x,y-4,z-8),new Vector3(7,14,6),sandstone);
        Cube("Deep carved brow",new Vector3(x,y+9,z-7),new Vector3(26,2.5f,1.2f),dark);
        Cube("Carved left eye",new Vector3(x-8,y+2,z-8),new Vector3(5,1.2f,1),dark);
        Cube("Carved right eye",new Vector3(x+8,y+2,z-8),new Vector3(5,1.2f,1),dark);
        Cube("Carved mouth",new Vector3(x,y-16,z-7),new Vector3(12,1.4f,1),dark);
        if(index==0)
        {
            for(int k=-2;k<=2;k++)Sphere("Hashirama long carved hair",new Vector3(x+k*7,y+27+Mathf.Abs(k)*2,z+1),new Vector3(11,23-Mathf.Abs(k)*3,10),sandstone);
            Sphere("Hashirama armor collar",new Vector3(x,y-23,z-1),new Vector3(45,10,8),sandstone);
        }
        if(index==1)
        {
            Sphere("Tobirama forehead protector",new Vector3(x,y+23,z),new Vector3(40,11,10),sandstone);
            for(int k=-1;k<=1;k++)Sphere("Tobirama spiked hair",new Vector3(x+k*12,y+34,z+2),new Vector3(10,17,9),sandstone);
            Sphere("Tobirama cheek marking",new Vector3(x-13,y-4,z-7),new Vector3(4,9,2),dark);
            Sphere("Tobirama cheek marking",new Vector3(x+13,y-4,z-7),new Vector3(4,9,2),dark);
        }
        if(index==2)
        {
            Sphere("Hiruzen Hokage hat",new Vector3(x,y+32,z+1),new Vector3(47,18,12),sandstone);
            Sphere("Hiruzen moustache left",new Vector3(x-9,y-9,z-9),new Vector3(15,5,3),sandstone);
            Sphere("Hiruzen moustache right",new Vector3(x+9,y-9,z-9),new Vector3(15,5,3),sandstone);
        }
        if(index==3)
        {
            for(int k=-2;k<=2;k++)Sphere("Minato spiked carved hair",new Vector3(x+k*7,y+28+Mathf.Abs(k),z+1),new Vector3(10,21-Mathf.Abs(k)*2,9),sandstone);
            Sphere("Minato headband",new Vector3(x,y+17,z-1),new Vector3(38,6,10),sandstone);
        }
        if(index==4)
        {
            Sphere("Tsunade hair bun left",new Vector3(x-22,y+21,z+1),new Vector3(13,15,11),sandstone);
            Sphere("Tsunade hair bun right",new Vector3(x+22,y+21,z+1),new Vector3(13,15,11),sandstone);
            var mark=Cube("Tsunade forehead diamond",new Vector3(x,y+13,z-7),new Vector3(4,6,1.3f),dark);mark.transform.rotation=Quaternion.Euler(0,0,45);
        }
        if(index==5)
        {
            Sphere("Kakashi carved headband",new Vector3(x,y+20,z-1),new Vector3(40,9,10),sandstone);
            for(int k=-2;k<=2;k++){var spike=Sphere("Kakashi swept spiky hair",new Vector3(x+k*7,y+29+Mathf.Abs(k)*2,z+2),new Vector3(11,18,10),sandstone);spike.transform.rotation=Quaternion.Euler(0,0,k*8);}
            Cube("Kakashi lower face mask",new Vector3(x,y-10,z-8),new Vector3(22,13,2),sandstone);
        }
        if(index==6)
        {
            for(int k=-3;k<=3;k++){var spike=Sphere("Naruto spiky carved hair",new Vector3(x+k*6.5f,y+27+Mathf.Abs(k)*2,z+1),new Vector3(9,19-Mathf.Abs(k),9),sandstone);spike.transform.rotation=Quaternion.Euler(0,0,k*7);}
            Sphere("Naruto forehead protector",new Vector3(x,y+17,z-1),new Vector3(37,7,10),sandstone);
            for(int side=-1;side<=1;side+=2)for(int line=0;line<3;line++)Cube("Naruto carved whisker mark",new Vector3(x+side*(13+line),y-5+line*3,z-8),new Vector3(6,1,1),dark);
        }
    }
    static GameObject Sphere(string n,Vector3 p,Vector3 scale,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name=n;g.transform.position=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;Register(g);return g;}
    static void MemorialStone()
    {
        var memorial=Cube("Ninja memorial stone",new Vector3(74,2,91),new Vector3(4,4,2),stone);
        var inscription=new GameObject("Memorial inscription");Register(inscription);inscription.transform.position=new Vector3(74,2,89.8f);inscription.transform.rotation=Quaternion.Euler(0,180,0);inscription.transform.localScale=new Vector3(-1,1,1);var tm=inscription.AddComponent<TextMesh>();tm.text="WILL OF FIRE";tm.fontSize=36;tm.characterSize=.12f;tm.anchor=TextAnchor.MiddleCenter;tm.color=Color.white;
    }
}
