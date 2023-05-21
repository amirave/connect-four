using System;
using System.Collections;
using UnityEngine;

namespace LlamAcademy.Spring.Runtime
{
    public class SpringToTarget3D : BaseSpringBehaviour, ISpringTo<Vector3>, INudgeable<Vector3>
    {
        private SpringVector3 Spring;

        private void Awake()
        {
            Spring = new SpringVector3()
            {
                StartValue = transform.position,
                EndValue = transform.position,
                Damping = Damping,
                Stiffness = Stiffness
            };
        }

        public void SpringTo(Vector3 TargetPosition, Action OnFinished)
        {
            StopAllCoroutines();

            CheckInspectorChanges();

            StartCoroutine(DoSpringToTarget(TargetPosition, OnFinished));
        }


        public void Nudge(Vector3 Amount)
        {
            CheckInspectorChanges();
            if (Mathf.Approximately(Spring.CurrentVelocity.sqrMagnitude, 0))
            {
                StartCoroutine(HandleNudge(Amount));
            }
            else
            {
                Spring.UpdateEndValue(Spring.EndValue, Spring.CurrentVelocity + Amount);
            }
        }

        private IEnumerator HandleNudge(Vector3 Amount)
        {
            Spring.Reset();
            Spring.StartValue = transform.position;
            Spring.EndValue = transform.position;
            Spring.InitialVelocity = Amount;
            Vector3 targetPosition = transform.position;
            transform.position = Spring.Evaluate(Time.deltaTime);

            while (!Mathf.Approximately(
                0,
                Vector3.SqrMagnitude(targetPosition - transform.position)
            ))
            {
                transform.position = Spring.Evaluate(Time.deltaTime);

                yield return null;
            }

            Spring.Reset();
        }

        private IEnumerator DoSpringToTarget(Vector3 TargetPosition, Action OnFinished)
        {
            if (Mathf.Approximately(Spring.CurrentVelocity.sqrMagnitude, 0))
            {
                Spring.Reset();
                Spring.StartValue = transform.position;
                Spring.EndValue = TargetPosition;
            }
            else
            {
                print(Vector3.SqrMagnitude(Spring.CurrentValue - Spring.EndValue));
                Spring.UpdateEndValue(TargetPosition, Spring.CurrentVelocity);
            }

            while (Vector3.SqrMagnitude(transform.position - TargetPosition) > 0.05f
                   || Vector3.SqrMagnitude(Spring.CurrentVelocity) > 0.01f)
            {
                transform.position = Spring.Evaluate(Time.deltaTime);

                yield return null;
            }

            Spring.Reset();
            OnFinished();
        }

        private void CheckInspectorChanges()
        {
            Spring.Damping = Damping;
            Spring.Stiffness = Stiffness;
        }
    }
}