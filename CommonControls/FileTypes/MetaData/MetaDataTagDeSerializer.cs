﻿using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommonControls.FileTypes.MetaData
{
    public class MetaDataTagDeSerializer
    {
        public static Dictionary<string, Type> _typeTable;
        public static Dictionary<string, string> _descriptionMap;

        static void EnsureMappingTableCreated()
        {
            if (_typeTable != null)
                return;

            _typeTable = new Dictionary<string, Type>();

            CreateDescriptions();

            var typesWithMyAttribute =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(MetaDataAttribute), true)
                where attributes != null && attributes.Length > 0
                select new { Type = t, Attributes = attributes.Cast<MetaDataAttribute>() };

            foreach (var instance in typesWithMyAttribute)
            {
                var type = instance.Type;
                var key = instance.Attributes.First().VersionName;
                _typeTable.Add(key, type);

                var orderedPropertiesList = type.GetProperties()
                    .Where(x => x.CanWrite)
                    .Where(x => Attribute.IsDefined(x, typeof(MetaDataTagAttribute)))
                    .OrderBy(x => x.GetCustomAttributes<MetaDataTagAttribute>(false).Single().Order)
                    .Select(x => x.GetCustomAttributes<MetaDataTagAttribute>(false).Single());

                var allNumbers = orderedPropertiesList.Select(x => x.Order).ToArray();
                if (IsSequential(allNumbers) == false)
                    throw new Exception("Invalid ids");

                // Ensure we have a decription
                GetDescription(instance.Attributes.First().Name);
            } 
        }

        static void CreateDescriptions()
        {
            _descriptionMap = new Dictionary<string, string>();
            _descriptionMap["TIME"] = "Generic time marker";
            _descriptionMap["DISABLE_PERSISTENT"] = "Disable persistent animation metadata";
            _descriptionMap["DISABLE_PERSISTENT_VFX"] = "Disable persistent vfx metadata";
            _descriptionMap["DISABLE_FACIAL"] = "Disable facial animations";
            _descriptionMap["DISABLE_HEAD_TRACKING"] = "Disable head tracking";
            _descriptionMap["DISABLE_MODEL"] = "Disable model draw.";
            _descriptionMap["SNIP"] = "Snip the starting and ending frame of an animation";
            _descriptionMap["FULL_BODY"] = "Indicates that this animation should be full body, that is, not torso spliced";
            _descriptionMap["NOT_BUILDING"] = "Indicates that this animation cannot be used for attacking buildings";
            _descriptionMap["USE_BASE_METADATA"] = "Force the use of metadata in the base anim slot during a hardcoded splice. All metadata in the hardcoded splice slot is ignored.";
            _descriptionMap["IGNORE_FOOT_SLIDING"] = "Mark the animation as not affected by foot sliding, so don't do anything to try to correct it.";
            _descriptionMap["WEAPON_ON"] = "Weapon bone is displayed between start and end times; set value to weapon bone index";
            _descriptionMap["WEAPON_LHAND"] = "Weapon bone is spliced relative to left hand between start and end times; set value to weapon bone index";
            _descriptionMap["WEAPON_RHAND"] = "Weapon bone is spliced relative to right hand between start and end times; set value to weapon bone index";
            _descriptionMap["WEAPON_HIP"] = "Weapon bone is spliced relative to hip between start and end times; set value to weapon bone index";
            _descriptionMap["DOCK_EQPT_LHAND"] = "missing";
            _descriptionMap["DOCK_EQPT_RHAND"] = "missing";
            _descriptionMap["DOCK_EQPT_LWAIST"] = "missing";
            _descriptionMap["DOCK_EQPT_RWAIST"] = "missing";
            _descriptionMap["DOCK_EQPT_BACK"] = "Weapon bone is spliced relative to spine2 between start and end times; set value to weapon bone index";
            _descriptionMap["BLEND_OVERRIDE"] = "Override the blend method and blend time";
            _descriptionMap["DISABLE_PERSISTENT_ID"] = "Disable persistent metadata with a particular ID";
            _descriptionMap["MIN_TARGET_SIZE"] = "Can only use this animation against target larger or equal to this size";
            _descriptionMap["MAX_TARGET_SIZE"] = "Can only use this animation against target smaller or equal to this size";
            _descriptionMap["RIDER_CUSTOM_ANIMATION"] = "force the rider to play a custom animation";
            _descriptionMap["BEARING"] = "Generic bearing in degrees";
            _descriptionMap["DISTANCE"] = "Distance in cm; currently used for jump animations";
            _descriptionMap["IMPACT_SPEED"] = "On attack animations, speed of the strike; On death animations, speed change threshold to trigger animation";
            _descriptionMap["SC_RADIUS"] = "Change the soft collision radius (multiplier)";
            _descriptionMap["SC_HEIGHT"] = "Change the soft collision height (multiplier)";
            _descriptionMap["SC_RATIO"] = "Change the soft collision ratio";
            _descriptionMap["ALPHA"] = "Change the alpha value";
            _descriptionMap["CAMERA_SHAKE_SCALE"] = "Set the camera shake scale";
            _descriptionMap["RIDER_IDLE_SPEED_SCALE"] = "scale the rider animation speed with some factor";
            _descriptionMap["RESCALE"] = "rescale character to target scale";
            _descriptionMap["ALLOWED_DELTA_SCALE"] = "maximum allowed delta scale to use the animation";
            _descriptionMap["PERSISTENT_SPEED_SCALE"] = "change the speed of the persistent metadata";
            _descriptionMap["BOUNDING_VOLUME_OVERRIDE"] = "A scale factor for the bounding sphere. This is a workaround for if the character animates far from the origin (or spreads out) and is being culled by the camera.";
            _descriptionMap["POSITION"] = "Generic position";
            _descriptionMap["FIRE_POS"] = "Position where projectile is created; start time is the time of projectile spawn";
            _descriptionMap["IMPACT_POS"] = "Position where impact originates; start time is the time of impact";
            _descriptionMap["TARGET_POS"] = "Indicates position of target";
            _descriptionMap["CAMERA_SHAKE_POS"] = "Start a camera shake at position";
            _descriptionMap["WOUNDED_POSE"] = "Specify the wounded pose in the last frame";
            _descriptionMap["LHAND_POSE"] = "Left hand pose key";
            _descriptionMap["RHAND_POSE"] = "Right hand pose key";
            _descriptionMap["FACE_POSE"] = "Face pose key";
            _descriptionMap["DISMEMBER"] = "Triggers dismembering at the start time";
            _descriptionMap["SPLICE"] = "Splices animation specified in metadata to this animation. Weights range from 0 to 1.";
            _descriptionMap["SPLICE_OVERRIDE"] = "Override hardcoded splice. Weights range from 0 to 1.";
            _descriptionMap["TRANSFORM"] = "Transform a node. Optionally override with another bone";
            _descriptionMap["CREW_LOCATION"] = "Position and face artillery crew";
            _descriptionMap["EFFECT"] = "Trigger a particle effect at a location relative to a node index";
            _descriptionMap["BLOOD"] = "Trigger a blood effect at a location relative to a node index";
            _descriptionMap["VOLUMETRIC_EFFECT"] = "Trigger a volumetric particle effect group";
            _descriptionMap["PROP"] = "Display a prop model at a location relative to a node index";
            _descriptionMap["ANIMATED_PROP"] = "Display an animated prop model at a location relative to a node index";
            _descriptionMap["RIDER_ATTACHMENT"] = "Mark an attachment point for a rider";
            _descriptionMap["TURRET_ATTACHMENT"] = "Mark an attachment point for a turret";
            _descriptionMap["SPLASH_ATTACK"] = "Trigger a splash attack and mark the area of effect";
            _descriptionMap["SOUND_IMPACT"] = "Time and with attack and defend types.";
            _descriptionMap["SOUND_ATTACK_TYPE"] = "Attack type of this combat animation.";
            _descriptionMap["SOUND_DEFEND_TYPE"] = "Defend type of this combat animation.";
            _descriptionMap["SOUND_SPHERE_LINK"] = "Link this entity with the spheres of matched combatants.";
            _descriptionMap["SOUND_BUILDING"] = "Time, event and position of sound to be triggered";
            _descriptionMap["SOUND_TRIGGER"] = "Time and type of sound to be triggered";
            _descriptionMap["SOUND_SPHERE"] = "Time and type of sound sphere to be triggered";
        }

        public static string GetDescription(string metaDataTagName)
        {
            EnsureMappingTableCreated();
            return _descriptionMap[metaDataTagName];
        }

        public static string GetDescriptionSafe(string metaDataTagName)
        {
            EnsureMappingTableCreated();
            if (_descriptionMap.ContainsKey(metaDataTagName) == false)
                return "Missing";
            return _descriptionMap[metaDataTagName];
        }

        public static List<string> GetSupportedTypes()
        {
            EnsureMappingTableCreated();
            return _typeTable.Select(x => x.Key).ToList();
        }

        static bool IsSequential(int[] array)
        {
            return array.Zip(array.Skip(1), (a, b) => (a + 1) == b).All(x => x);
        }

        static Type GetTypeFromMeta(BaseMetaEntry entry)
        {
            EnsureMappingTableCreated();

            var key = entry.Name + "_" + entry.Version;
            if (_typeTable.ContainsKey(key) == false)
                return null;

            return _typeTable[key];
        }

        public static DecodedMetaEntryBase DeSerialize(UnknownMetaEntry entry)
        {
            EnsureMappingTableCreated();

            var entryInfo = GetEntryInformation(entry);
            var instance = Activator.CreateInstance(entryInfo.type);
            var bytes = entry.Data;
            int currentIndex = 0;
            foreach (var proptery in entryInfo.Properties)
            {
                var parser = ByteParserFactory.Create(proptery.PropertyType);
                var value = parser.GetValueAsObject(bytes, currentIndex, out var bytesRead);
                currentIndex += bytesRead;
                proptery.SetValue(instance, value);
            }

            if (bytes.Length != currentIndex)
                throw new Exception("Failed to read object - bytes left");

            var typedInstance = instance as DecodedMetaEntryBase;
            typedInstance.Name = entry.Name;
            typedInstance.Data = bytes;
            return typedInstance;
        }

        internal static DecodedMetaEntryBase CreateDefault(string itemName)
        {
            EnsureMappingTableCreated();

            if (_typeTable.ContainsKey(itemName) == false)
                throw new Exception("Unkown metadata item " + itemName);

            var instance = Activator.CreateInstance(_typeTable[itemName]) as DecodedMetaEntryBase;

            var itemNameSplit = itemName.ToUpper().Split("_");
            instance.Version = int.Parse(itemNameSplit.Last());
            return instance;
        }
        
        public static List<(string Header, string Value)> DeSerializeToStrings(BaseMetaEntry entry)
        {
            var entryInfo = GetEntryInformation(entry);
            var bytes = entry.Data;
            int currentIndex = 0;
            var output = new List<(string, string)>();

            foreach (var proptery in entryInfo.Properties)
            {
                var parser = ByteParserFactory.Create(proptery.PropertyType);
                var result = parser.TryDecode(bytes, currentIndex, out var value, out var bytesRead, out var error);
                if (result == false)
                    throw new Exception($"Failed to serialize {proptery.Name} - {error}");
                currentIndex += bytesRead;

                output.Add((proptery.Name, value));
            }

            if (bytes.Length != currentIndex)
                throw new Exception("Failed to read object - bytes left");

            return output;
        }

        static (Type type, List<PropertyInfo> Properties) GetEntryInformation(BaseMetaEntry entry)
        {
            var metaDataType = GetTypeFromMeta(entry);
            if (metaDataType == null)
                throw new Exception($"Unable to find decoder for {entry.Name} _ {entry.Version}");

            var instance = Activator.CreateInstance(metaDataType);
            var orderedPropertiesList = metaDataType.GetProperties()
                .Where(x => x.CanWrite)
                .Where(x => Attribute.IsDefined(x, typeof(MetaDataTagAttribute)))
                .OrderBy(x => x.GetCustomAttributes<MetaDataTagAttribute>(false).Single().Order)
                .ToList();

            return (metaDataType, orderedPropertiesList);
        }
    }
}