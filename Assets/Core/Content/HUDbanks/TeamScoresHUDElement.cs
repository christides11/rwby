using System.Collections;
using System.Collections.Generic;
using System.Text;
using rwby;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI.Extensions;

namespace rwby
{
    public class TeamScoresHUDElement : HUDElement
    {
        public TeamScoreElement teamScoreElementPrefab;
        public Transform contentHolder;

        private ITeamScoreProvider scoreProvider;

        private Dictionary<int, TeamScoreElement> teamScoreElements = new Dictionary<int, TeamScoreElement>();

        public override void InitializeElement(BaseHUD parentHUD)
        {
            scoreProvider = (ITeamScoreProvider)GameModeBase.singleton;
        }

        public override void UpdateElement(BaseHUD parentHUD)
        {
            int currTeam = 0;
            foreach (var item in scoreProvider.TeamScores)
            {
                if (!teamScoreElements.ContainsKey(currTeam))
                {
                    var go = GameObject.Instantiate(teamScoreElementPrefab, contentHolder, false);
                    teamScoreElements.Add(currTeam, go);
                    go.gameObject.SetActive(true);
                }

                if (item != teamScoreElements[currTeam].currentScore) teamScoreElements[currTeam].UpdateValue(item, scoreProvider.MaxScore);
                currTeam++;
            }
        }
    }
}