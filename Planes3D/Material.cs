using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planes3D
{
    //https://github.com/lwjglgamedev/lwjglbook/blob/master/chapter27/c27-p2/src/main/java/org/lwjglb/engine/graph/Material.java
    public class Material
    {

        public static Vector3 DEFAULT_COLOUR = new Vector3(1.0f, 1.0f, 1.0f);

        private Vector3 ambientColour;

        private Vector3 diffuseColour;

        private Vector3 specularColour;

        private float shininess;

        private float reflectance;

        private Texture texture;

        public Material()
        {
            this.ambientColour = DEFAULT_COLOUR;
            this.diffuseColour = DEFAULT_COLOUR;
            this.specularColour = DEFAULT_COLOUR;
            this.texture = null;
            this.reflectance = 0;
        }

        public Material(Vector3 colour, float reflectance) : this(colour, colour, colour, null, reflectance)
        {
        }

        public Material(Texture texture) : this(DEFAULT_COLOUR, DEFAULT_COLOUR, DEFAULT_COLOUR, texture, 0)
        {
        }

        public Material(Texture texture, float reflectance) : this(DEFAULT_COLOUR, DEFAULT_COLOUR, DEFAULT_COLOUR, texture, reflectance)
        {
         
        }

        public Material(Vector3 ambientColour, Vector3 diffuseColour, Vector3 specularColour, float reflectance) : this(ambientColour, diffuseColour, specularColour, null, reflectance)
        {
        }

        public Material(Vector3 ambientColour, Vector3 diffuseColour, Vector3 specularColour, Texture texture, float reflectance)
        {
            this.ambientColour = ambientColour;
            this.diffuseColour = diffuseColour;
            this.specularColour = specularColour;
            this.texture = texture;
            this.reflectance = reflectance;
        }

        public Vector3 getAmbientColour()
        {
            return ambientColour;
        }

        public void setAmbientColour(Vector3 ambientColour)
        {
            this.ambientColour = ambientColour;
        }

        public Vector3 getDiffuseColour()
        {
            return diffuseColour;
        }

        public void setDiffuseColour(Vector3 diffuseColour)
        {
            this.diffuseColour = diffuseColour;
        }

        public Vector3 getSpecularColour()
        {
            return specularColour;
        }

        public void setSpecularColour(Vector3 specularColour)
        {
            this.specularColour = specularColour;
        }

        public float getReflectance()
        {
            return reflectance;
        }

        public void setReflectance(float reflectance)
        {
            this.reflectance = reflectance;
        }

        public bool isTextured()
        {
            return this.texture != null;
        }

        public Texture getTexture()
        {
            return texture;
        }

        public void setTexture(Texture texture)
        {
            this.texture = texture;
        }

    }
}
