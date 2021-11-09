/*
 * Copyright (c) 2021 HookedBehemoth
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms and conditions of the GNU General Public License,
 * version 3, as published by the Free Software Foundation.
 *
 * This program is distributed in the hope it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using VRC.Udon;
using System;
using System.Collections.Generic;

namespace FakeUdon {
    public interface UdonMatcher {
        bool Match(UdonBehaviour behaviour);
    }
    public class UdonNameMatcher : UdonMatcher {
        private string _name;
        public UdonNameMatcher(string name) {
            _name = name;
        }
        public bool Match(UdonBehaviour behaviour)
            => behaviour.name == _name;
    }
    public class UdonProgramAssetMatcher : UdonMatcher {
        private string _assetName;
        public UdonProgramAssetMatcher(string assetName) {
            _assetName = assetName;
        }
        public bool Match(UdonBehaviour behaviour)
            => behaviour.serializedProgramAsset?.name == _assetName;
    }
    public static class FakeUdonRegistry {
        private static readonly List<(UdonMatcher, Type)> BehaviourTypes = new List<(UdonMatcher, Type)>();
        public static void RegisterType<T>(UdonMatcher matcher) where T : UdonSharp.UdonSharpBehaviour {
            BehaviourTypes.Add((matcher, typeof(T)));
        }
        public static void RegisterType<T>(string assetName) where T : UdonSharp.UdonSharpBehaviour {
            BehaviourTypes.Add((new UdonProgramAssetMatcher(assetName), typeof(T)));
        }
        public static bool Find(UdonBehaviour behaviour, out Type type) {
            foreach (var (matcher, type_) in BehaviourTypes) {
                if (matcher.Match(behaviour)) {
                    type = type_;
                    return true;
                }
            }
            type = null;
            return false;
        }
    }
}
