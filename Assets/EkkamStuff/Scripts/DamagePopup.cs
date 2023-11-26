using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Animations;

namespace Ekkam {
    public class DamagePopup : MonoBehaviour
    {
        TMP_Text damageText;
        float disappearTimer;

        float moveSpeed = 30f; // how fast the popup moves up
        float disappearTime = 0.3f; // how long the popup lasts before disappearing
        float disappearSpeed = 4f; // how fast the popup disappears

        void Awake()
        {
            damageText = GetComponent<TMP_Text>();
            RotationConstraint rc = GetComponent<RotationConstraint>();
            rc.AddSource(new ConstraintSource() { sourceTransform = Camera.main.transform, weight = 1f });
        }

        public void SetDamageText(float damage, bool isCriticalHit)
        {
            damageText.text = damage.ToString();
            if (isCriticalHit)
            {
                damageText.fontSize = 65f;
                damageText.color = new Color(1f, 0.5f, 0f);
            }
        }

        void Update()
        {
            disappearTimer += Time.deltaTime;
            transform.position += Vector3.up * Time.deltaTime * moveSpeed;
            if (disappearTimer >= disappearTime)
            {
                transform.localScale -= Vector3.one * Time.deltaTime * disappearSpeed;
                if (transform.localScale.x <= 0f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
