using System.Collections.Generic;
using UnityEngine;

public struct Waypoint {
    public enum Type {
        Path, Transition, Terminus
    }

    public class Nodes {
        public static Dictionary<Type, GameObject> nodes = new Dictionary<Type, GameObject>() {
            { Type.Path, Resources.Load<GameObject>("PathNode")},
            { Type.Transition, Resources.Load<GameObject>("TransitionNode") },
            { Type.Terminus, Resources.Load<GameObject>("TerminusNode") }
        };
    }

    public class Internal {
        public Type type;
        public Vector2 position;

        public Internal(Type type, Vector2 position) {
            this.type = type;
            this.position = position;
        }
    }

    public class Visual : Internal {
        private GameObject node;

        public Visual(Type type, Vector2 position, Transform waypointHolder, bool isVisible = true): base(type, position) {
            this.type = type;
            this.position = position;

            node = Object.Instantiate(Nodes.nodes[type], position, Quaternion.identity, waypointHolder);
            SetVisibility(isVisible);
        }

        public void SetVisibility(bool visible) {
            node.SetActive(visible);
        }

        public void DeleteNode() {
            Object.Destroy(node);
        }
    }
}
